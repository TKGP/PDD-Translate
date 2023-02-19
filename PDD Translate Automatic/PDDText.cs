
// Undefine to use slow-ass page scraping if this ever breaks
#define USE_SECRET_API

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace PDD_Translate_Automatic
{
    class PDDText
    {
        // Times to try failed web request
        private const int TRIES_NUM = 3;
        // Delay between tries
        private const int TRIES_WAIT = 100;
        // Max characters for Google request (after encoding)
        private const int GOOGLE_LIMIT = 2015;
        // Max characters for Microsoft request (before encoding)
        private const int MS_LIMIT = 5000;

        private static AzureAuthToken authToken;
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, TranslateItem>> translations = new ConcurrentDictionary<string, ConcurrentDictionary<string, TranslateItem>>();
        private static List<string> stringIDs;

        private static Regex cyrillicRx = new Regex("[АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ]", RegexOptions.IgnoreCase);
        private static Regex rawQuoteRx = new Regex(@"(?<!\\)""");

        // For machine translation
        private static Regex whitespacePattern = new Regex(@"^(\s*).+?(\s*)$");
        private static Regex punctCyrillic = new Regex(@"([\.\!\?])([АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ])");
        private static Regex[] halveBreaks = {
            new Regex(@"[\.\!\?] \w"),
            new Regex(@"\; \w"),
            new Regex(@"\: \w"),
            new Regex(@"\, \w")
            };
        // For super secret Google Translate API
        private static Regex gtJsonExtract = new Regex(@"\[""(.+?)(?<!\\)"","".+?(?<!\\)"",null,null,\d+");
        private static Regex gtFailure = new Regex(@"\[\[\["""","".+?(?<!\\)""\]\],,""ru""\]");
        // For scraping text from Google Translate full page
        private static Regex googleScrape = new Regex("<span[^>]+?result_box[^>]*?>(.+?)</span></div>");
        private static Regex removeTags = new Regex("<.+?>(?!\")");

        private static Regex[] splitPoints = {
            new Regex(@"\r?\n"),
            new Regex(@"\\?\\n"),
            new Regex(@"\$\$.+?\$\$"), // Keybind macro
            new Regex(@"%c\[[^]]+?]"), // Color code
            new Regex(@"(?<!%)%(?:([1-9]\d*)\$|\(([^\)]+)\))?(\+)?(0|'[^$])?(-)?(\d+)?(?:\.(\d+))?([aAdeEfFgGinopsuxX])"), // string.format placeholders
            new Regex(@"(?<!\$)\$\w+") // AMK-style placeholders
            };

        public static void Init()
        {
            string token = File.ReadAllText(@"res\mst token.txt");
            authToken = new AzureAuthToken(token.Trim());
        }

        public static void GetStringIDs(string configPath)
        {
            configPath += @"\text\rus";
            stringIDs = new List<string>();
            if (Directory.Exists(configPath))
            {
                foreach (string filepath in Directory.GetFiles(configPath, "*.xml", SearchOption.AllDirectories))
                {
                    foreach (Match match in Regex.Matches(File.ReadAllText(filepath), "<string id=\"([^\"]+)\">"))
                    {
                        stringIDs.Add(match.Groups[1].Value);
                        if (cyrillicRx.Match(match.Groups[1].Value).Success)
                            ProgressForm.Bug("Found cyrillic ID: " + match.Groups[1].Value);
                    }
                }
            }
        }

        public static string TranslateMatch(Match match, string extension)
        {
            if (!cyrillicRx.Match(match.Value).Success || stringIDs.Contains(match.Value))
                return null;

            Group textGroup = match.Groups["text"];
            string plainText = textGroup.Value;
            string left = match.Value.Substring(0, textGroup.Index - match.Index);
            string right = match.Value.Substring(textGroup.Index - match.Index + textGroup.Length);

            // Pre-processing
            if (extension == ".ltx")
            {
                if (Regex.Match(plainText, @"\{[+-][^\}]+\}").Success)
                {
                    ProgressForm.Bug("Aborting for condlist: " + plainText);
                    return null;
                }
                if (plainText[0] == '"' && plainText[plainText.Length - 1] == '"')
                    plainText = plainText.Substring(1, plainText.Length - 2);
            }
            else if (extension == ".xml")
                plainText = HttpUtility.HtmlDecode(plainText);
            else if (extension == ".script")
            {
                plainText = plainText.Replace("\\\"", "\"");
            }

            string result = TranslateString(plainText, extension);

            // Post-processing
            if (extension == ".ltx")
            {
                result = result.Replace("\"", "'");
                if (Regex.Match(result, @"\s").Success)
                    result = "\"" + result + "\"";
            }
            else if (extension == ".xml")
            {
                result = SecurityElement.Escape(result);
                // Xray crashes with "dest string less than needed" if lines are too long
                bool success;
                do
                {
                    success = true;
                    foreach (Match line in Regex.Matches(result, @"[^\r\n]+"))
                    {
                        if (line.Length > 4000)
                        {
                            string leftHalf, rightHalf;
                            HalveText(line.Value, out leftHalf, out rightHalf);
                            result = (line.Index > 0 ? result.Substring(0, line.Index) : "") + leftHalf + "\r\n" + rightHalf
                                    + (line.Index + line.Length < result.Length ? result.Substring(line.Index + line.Length) : "");
                            success = false;
                            break;
                        }
                    }
                } while (!success);
            }
            else if (extension == ".script")
            {
                left = "\"";
                right = "\"";
                result = rawQuoteRx.Replace(result, @"\""");
            }

            return left + result + right;
        }

        private static string TranslateSegment(string input, string extension)
        {
            if (!cyrillicRx.Match(input).Success)
                return input;

            string result;
            if (PDDCorpus.GetFromCorpus(input, out result))
            {
                ProgressForm.AddCorpus(input.Length);
                return result;
            }

            string lang = PDDLanguage.Current.GoogleCode;
            lock (translations)
            {
                if (!translations.ContainsKey(input))
                    translations[input] = new ConcurrentDictionary<string, TranslateItem>();
                if (!translations[input].ContainsKey(lang))
                    translations[input][lang] = new TranslateItem();
            }

            lock (translations[input][lang].Locker)
            {
                if (translations[input][lang].Done)
                    return translations[input][lang].Result;

                if (extension == ".script")
                {
                    result = input.Replace("%%", "%");
                    result = MachineTranslate(result);
                    result = result.Replace("%", "%%");
                }
                else
                    result = MachineTranslate(input);

                translations[input][lang].Finish(result);
                return result;
            }
        }

        private static string TranslateString(string input, string extension)
        {
            if (!cyrillicRx.Match(input).Success)
                return input;

            string result;
            if (PDDCorpus.GetFromCorpus(input, out result))
            {
                ProgressForm.AddCorpus(input.Length);
                return result;
            }

            List<Match> matches = new List<Match>();
            foreach (Regex pattern in splitPoints)
                foreach (Match match in pattern.Matches(input))
                    matches.Add(match);
            matches.Sort((m1, m2) => m1.Index.CompareTo(m2.Index));
            int nextIndex = 0;
            StringBuilder resultSb = new StringBuilder();
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                if (match.Index > 0)
                    resultSb.Append(TranslateSegment(input.Substring(nextIndex, match.Index - nextIndex), extension));
                resultSb.Append(match.Value);
                nextIndex = match.Index + match.Length;
            }
            if (nextIndex < input.Length)
                resultSb.Append(TranslateSegment(input.Substring(nextIndex, input.Length - nextIndex), extension));
            return resultSb.ToString();
        }

        private static string AddSpace(Match match)
        {
            return match.Groups[1].Value + " " + match.Groups[2].Value;
        }

        private static string MachineTranslate(string toTranslate)
        {
            string sanitized = toTranslate;
            // Leading and trailing whitespace is stripped and added back later
            Match whitespace = whitespacePattern.Match(sanitized);
            sanitized = sanitized.Trim();
            // Google doesn't always handle these well
            sanitized = sanitized.Replace("«", "\"")
                .Replace("»", "\"")
                .Replace("№", "#");
            // Add spaces after punctuation
            sanitized = punctCyrillic.Replace(sanitized, AddSpace);

            string result = ChunkText(sanitized);
            result = whitespace.Groups[1].Value + result + whitespace.Groups[2].Value;
            return result;
        }

        private static string ChunkText(string input)
        {
            string urlEncoded = HttpUtility.UrlEncode(input);
            if (urlEncoded.Length > GOOGLE_LIMIT)
            {
                if (HalveText(input, out string left, out string right))
                    return ChunkText(left) + " " + ChunkText(right);
            }
            return RequestTranslation(input);
        }

        private static bool HalveText(string text, out string left, out string right)
        {
            int bestPunct = -1;
            int halfway = (text.Length - 1) / 2;
            foreach (Regex textBreak in halveBreaks)
            {
                foreach (Match match in textBreak.Matches(text))
                    if (Math.Abs(match.Index - halfway) < Math.Abs(bestPunct - halfway))
                        bestPunct = match.Index;
                if (bestPunct != -1)
                    break;
            }
            if (bestPunct != -1)
            {
                left = text.Substring(0, bestPunct + 1);
                right = text.Substring(bestPunct + 2);
                return true;
            }
            else
            {
                left = null;
                right = null;
                return false;
            }
        }

        private static string RequestTranslation(string text)
        {
            ProgressForm.AddGoogle(text.Length);
            if (PDDOptions.DemoMode)
                return text;
            string result;
            if (GoogleTranslate(text, out result))
                return result;
            if (MSTranslate(text, out result))
                return result;
            ProgressForm.Bug("What an exceptionally long string!");
            return InterruptForm.GetInterrupt(text, PDDLanguage.Current.Name);
        }

        private static bool GoogleTranslate(string input, out string result)
        {
            result = "";
            string urlEncoded = HttpUtility.UrlEncode(input);
            if (urlEncoded.Length > GOOGLE_LIMIT)
                return false;
            string source = PDDLanguage.Russian.GoogleCode;
            string target = PDDLanguage.Current.GoogleCode;
            // Non-Json URL: "http://translate.googleapis.com/translate_a/t?client=gtx&sl={0}&tl={1}&q={2}"
            // Unfortunately this stops working after a few requests
            string format = "http://translate.googleapis.com/translate_a/single?client=gtx&dt=t&ie=UTF-8&oe=UTF-8&sl={0}&tl={1}&q={2}";
            string url = string.Format(format, source, target, input);

            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            string output = null;
            for (int tries = 1; tries <= TRIES_NUM; tries++)
            {
                try
                {
                    output = webClient.DownloadString(url);
                    if (gtFailure.Match(output).Success)
                    {
                        ProgressForm.Bug("Random Google failure, retrying...");
                        continue;
                    }
                }
                catch (WebException)
                {
                    ProgressForm.Bug("Google request failed.");
                    Thread.Sleep(TRIES_WAIT);
                }
            }
            if (output == null)
                return false;

            //foreach (Match match in gtJsonExtract.Matches(output))
            //    result += match.Groups[1].Value;

            dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(output);
            if (deserialized.Count != 8)
                throw new InvalidDataException();

            foreach (dynamic translationElement in deserialized[0])
            {
                if (translationElement.Count < 5)
                    throw new InvalidDataException();

                result += translationElement[0];
            }

            result = result.Replace("\\\"", "\"")
                .Replace(@"\/", "/")
                .Replace(@"\r", "\r")
                .Replace(@"\n", "\n")
                .Replace(@"\\", @"\");

            if (result == "")
            {
                ProgressForm.Bug("Failed to capture result. Input:\r\n" + input + "\r\nOutput:\r\n" + output);
                return false;
            }
            else
                return true;
        }

        private static bool MSTranslate(string input, out string result)
        {
            result = "";
            if (input.Length > MS_LIMIT)
                return false;
            string urlEncoded = HttpUtility.UrlEncode(input);
            string format = "http://api.microsofttranslator.com/v2/Http.svc/Translate?from={0}&to={1}&text={0}";
            string uri = string.Format(format, urlEncoded, PDDLanguage.Russian.GoogleCode, PDDLanguage.Current.GoogleCode);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            string accessToken = null;
            for (int tries = 1; tries <= 5; tries++)
            {
                try
                {
                    accessToken = authToken.GetAccessToken();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
            if (accessToken != null)
            {
                httpWebRequest.Headers.Add("Authorization", accessToken);
                httpWebRequest.Timeout = 10000;
                for (int tries = 1; tries <= TRIES_NUM; tries++)
                {
                    try
                    {
                        using (WebResponse response = httpWebRequest.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                                result = (string)dcs.ReadObject(stream);
                                return true;
                            }
                        }
                    }
                    catch (WebException)
                    {
                        Thread.Sleep(TRIES_WAIT);
                    }
                }
            }
            ProgressForm.Bug("MS request failed.");
            return false;
        }
    }
}
