using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

namespace PDDTranslate
{
    class Translator
    {
        public enum TranslateOption { Skip, Manual, Semi, Auto };
        public enum ContinueOption { ConfirmCorpus, ConfirmNoCorpus, Skip };
        private static readonly Encoding RUENCODING = Encoding.GetEncoding(1251);

        private static TranslateForm translateForm;
        private static TranslateOption scriptOption, ltxOption, xmlOption, stringOption;
        private static ContinueOption continueOption;
        private static string textInput;
        public static string InterruptResult;

        private static Thread fileParser;
        private static EventWaitHandle waitHandle = new AutoResetEvent(false);
        private static AdmAuthentication admAuth;
        
        private static Dictionary<string, string> staticCorpus = new Dictionary<string, string>();
        private static Dictionary<string, string> customCorpus = new Dictionary<string, string>();
        private static Dictionary<string, string> tempCorpus = new Dictionary<string, string>();
        private static Dictionary<string, string> machineCache = new Dictionary<string, string>();
        private static XmlDocument customCorpusXML = new XmlDocument();

        private static Dictionary<string, string> doneFiles = new Dictionary<string, string>();
        private static MatchCollection comments, moreComments;
        private static TranslateOption currentMode;
        private static string fileType, currentFile;
        private static bool fromCorpus;
        
        private static Regex anyCyrillic = new Regex("[АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ]", RegexOptions.IgnoreCase);
        private static Regex whitespacePattern = new Regex(@"^(\s*).+?(\s*)$");
        private static Regex dotCyrillic = new Regex(@"\.([АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ])");
        private static Regex googleScrape = new Regex("<span[^>]+?result_box[^>]*?>(.+?)</span></div>");
        private static Regex removeTags = new Regex("<.+?>(?!\")");

        private static Regex scriptText = new Regex(@"([""'])(?<text>(?:[^\1\n]|\\\1)*?)(?<!\\)\1");
        private static Regex ltxText = new Regex(@"(?:inv_name|inv_name_short|description|title|descr)\s*?=\s*(?<text>[^;\n]+[^;\s])");
        private static Regex textText = new Regex(@"<text[^>]*?>(?<text>[^<]*?)</text>");
        private static Regex attributeText = new Regex("<[^>]*?(?:hint|name|caption)=\"(?<text>[^\"]*?)\"");
        private static Regex gameplayText = new Regex(@"<(name|title|text)>(?<text>[^<]*?)</\1>");

        private static Regex[] splitPoints = {
            new Regex(@"\. "),
            new Regex(@"\n"),
            new Regex(@"\\?\\n"),
            new Regex(@"\$\$.+?\$\$"), // Keybind macro
            new Regex(@"%c\[[^]]+?]"), // Color code
            new Regex(@"%(?:[\.\d]+\$)?[dfsu]"), // string.format arguments
            new Regex(@"\$\S+") // AMK-style placeholders
            };

        private static MatchEvaluator matchEvaluator = new MatchEvaluator(ProcessMatch);

        public static void SetParentGUI(TranslateForm setParentGUI)
        {
            translateForm = setParentGUI;
        }

        public static void SetTranslateOptions(TranslateOption setScriptOption, TranslateOption setLtxOption, TranslateOption setXmlOption, TranslateOption setStringOption)
        {
            scriptOption = setScriptOption;
            ltxOption = setLtxOption;
            xmlOption = setXmlOption;
            stringOption = setStringOption;
        }

        public static void Start()
        {
            fileParser = new Thread(new ThreadStart(ParseFiles));
            fileParser.SetApartmentState(ApartmentState.STA);
            fileParser.IsBackground = true;
            fileParser.Start();
        }

        public static void Continue(ContinueOption setContinueOption, string setTextInput)
        {
            continueOption = setContinueOption;
            textInput = setTextInput;
            waitHandle.Set();
        }

        private static void ParseFiles()
        {
            string token = File.ReadAllText("token.txt");
            admAuth = new AdmAuthentication("PDDTranslate", token);

            LoadCorpus(staticCorpus, "sgm 2.2.xml");
            LoadCorpus(staticCorpus, "amk retranslated.xml");
            LoadCorpus(staticCorpus, "lost alpha.xml");
            LoadCorpus(staticCorpus, "shadow of chernobyl.xml");
            LoadCorpus(staticCorpus, "clear sky.xml");
            LoadCorpus(staticCorpus, "call of pripyat.xml");
            LoadCorpus(customCorpus, "custom.xml");
            customCorpusXML.Load("corpus\\custom.xml");

            if (Directory.Exists("output"))
                Directory.Delete("output", true);
            Directory.CreateDirectory("output");
            string configPath = Directory.Exists(@"input\config") ? "config" : "configs";
            int howMany;


            translateForm.SetStatus((scriptOption != TranslateOption.Skip ? "Checking" : "Skipping") + " scripts...\t\t(1/4)");
            howMany = IterateFiles(scriptOption, "scripts", "*.script", scriptText);
            if (howMany > -1) translateForm.SetStatus("Translated " + howMany + " files.");

            translateForm.SetStatus((ltxOption != TranslateOption.Skip ? "Checking" : "Skipping") + " ini files...\t\t(2/4)");
            howMany = IterateFiles(ltxOption, configPath, "*.ltx", ltxText);
            if (howMany > -1) translateForm.SetStatus("Translated " + howMany + " files.");

            translateForm.SetStatus((xmlOption != TranslateOption.Skip ? "Checking" : "Skipping") + " loose xml...\t(3/4)");
            howMany = IterateFiles(xmlOption, configPath + @"\gameplay", "*.xml", gameplayText);
            howMany += IterateFiles(xmlOption, configPath + @"\gameplay", "*.xml", attributeText);
            howMany += IterateFiles(xmlOption, configPath + @"\ui", "*.xml", textText);
            howMany += IterateFiles(xmlOption, configPath + @"\ui", "*.xml", attributeText);
            if (howMany > -1) translateForm.SetStatus("Translated " + howMany + " files.");

            translateForm.SetStatus((stringOption != TranslateOption.Skip ? "Checking" : "Skipping") + " string tables...\t(4/4)");
            howMany = IterateFiles(stringOption, configPath + @"\text\rus", "*.xml", textText);
            if (howMany > -1) translateForm.SetStatus("Translated " + howMany + " files.");

            translateForm.DisableControls();
            translateForm.SetStatus("Done!");
            SystemSounds.Asterisk.Play();
        }

        private static int IterateFiles(TranslateOption translateOption, string path, string filePattern, Regex pattern)
        {
            if (translateOption == TranslateOption.Skip)
                return -1;
            if (translateOption == TranslateOption.Auto)
                translateForm.DisableControls();
            else
                translateForm.EnableControls();
            if (!Directory.Exists(@"input\" + path))
            {
                translateForm.SetStatus(@"Directory input\" + path + " not found, skipping...");
                return -1;
            }
            int fileCount = 0;
            string[] filePaths = Directory.GetFiles(@"input\" + path + @"\", filePattern, SearchOption.AllDirectories);
            currentMode = translateOption;
            int howMany = 0;
            foreach (string filePath in filePaths)
            {
                currentFile = Regex.Match(filePath, @".+\\(.+)").Groups[1].Value;
                fileType = Regex.Match(currentFile, @".+\.(.+)").Groups[1].Value.ToLower();
                string fileText, newText;
                fileCount++;
                currentFile = Regex.Match(filePath, @".+\\(.+)").Groups[1].Value;
                translateForm.SetCurrentFile(filePath + " (" + fileCount + "/" + filePaths.Length + ")");
                if (doneFiles.ContainsKey(filePath))
                    fileText = doneFiles[filePath];
                else
                    fileText = File.ReadAllText(filePath, RUENCODING);
                translateForm.SetFileText(fileText);

                // Find all comments to prune overlapping matches later
                moreComments = Regex.Matches("a","b"); // Hack to make empty MatchCollection
                switch (fileType)
                {
                    case "ltx":
                        comments = Regex.Matches(fileText, ";.+");
                        break;
                    case "xml":
                        comments = Regex.Matches(fileText, "<!--.+?-->", RegexOptions.Singleline);
                        break;
                    case "script":
                        comments = Regex.Matches(fileText, @"--(?!\[\[).+");
                        moreComments = Regex.Matches(fileText, @"--\[\[.+?\]\]", RegexOptions.Singleline);
                        break;
                    default:
                        continue; // GetFiles does some bullshit where *.ltx returns file.ltx~ (for instance) so avoid that
                }

                newText = pattern.Replace(fileText, matchEvaluator);
                if (newText != fileText)
                {
                    howMany++;
                    doneFiles[filePath] = newText;
                    string where = @"output\" + Regex.Match(filePath.Substring(5), @".+\\");
                    Directory.CreateDirectory(where);
                    File.WriteAllText(@"output\" + filePath.Substring(5), newText, RUENCODING);
                }
            }
            return howMany;
        }

        private static string ProcessMatch(Match fullMatch)
        {
            Group textGroup = fullMatch.Groups["text"];

            if (!anyCyrillic.Match(textGroup.Value).Success)
                return fullMatch.Value;

            foreach (Match comment in comments)
            {
                if ((fullMatch.Index >= comment.Index && fullMatch.Index <= comment.Index + comment.Length)
                    || (fullMatch.Index + fullMatch.Length >= comment.Index && fullMatch.Index + fullMatch.Length <= comment.Index + comment.Length))
                    return fullMatch.Value;
                else if (comment.Index >= fullMatch.Index && comment.Index <= fullMatch.Index + fullMatch.Length)
                    translateForm.AddLog("Comment inside string?!", currentFile);
            }
            foreach (Match comment in moreComments)
            {
                if ((fullMatch.Index >= comment.Index && fullMatch.Index <= comment.Index + comment.Length)
                    || (fullMatch.Index + fullMatch.Length >= comment.Index && fullMatch.Index + fullMatch.Length <= comment.Index + comment.Length))
                    return fullMatch.Value;
                else if (comment.Index >= fullMatch.Index && comment.Index <= fullMatch.Index + fullMatch.Length)
                    translateForm.AddLog("Comment inside string?!", currentFile);
            }

            translateForm.ScrollFile(textGroup.Index, textGroup.Length);

            string plainText = textGroup.Value;
            string left = fullMatch.Value.Substring(0, textGroup.Index - fullMatch.Index);
            string right = fullMatch.Value.Substring(textGroup.Index - fullMatch.Index + textGroup.Length);

            // Pre-processing
            switch (fileType)
            {
                case "ltx":
                    if (plainText[0] == '"')
                        plainText = plainText.Substring(1);
                    if (plainText[plainText.Length - 1] == '"')
                        plainText = plainText.Substring(0, plainText.Length - 1);
                    break;
                case "xml":
                    plainText = HttpUtility.HtmlDecode(plainText);
                    break;
                case "script":
                    plainText = plainText.Replace("\\\"", "\"");
                    break;
            }

            string result;
            fromCorpus = true;
            if (CheckCorpus(plainText))
                result = GetCorpus(plainText);
            else
                result = Split(plainText);

            if (currentMode == TranslateOption.Manual || (currentMode == TranslateOption.Semi && !fromCorpus ))
            {
                Clipboard.SetText(textGroup.Value);
                translateForm.SetSuggestion(textGroup.Value, result);

                waitHandle.WaitOne(); // Wait for user input

                result = textInput;
                switch (continueOption)
                {
                    case ContinueOption.Skip:
                        return fullMatch.Value;
                    case ContinueOption.ConfirmCorpus:
                        customCorpus[plainText] = result;
                        XmlNode corpusNode = customCorpusXML.DocumentElement.AppendChild(customCorpusXML.CreateElement("text"));
                        corpusNode.AppendChild(customCorpusXML.CreateElement("rus")).InnerText = plainText;
                        corpusNode.AppendChild(customCorpusXML.CreateElement("eng")).InnerText = result;
                        customCorpusXML.Save("corpus\\custom.xml");
                        break;
                    case ContinueOption.ConfirmNoCorpus:
                        tempCorpus[plainText] = result;
                        break;
                }
            }

            // Post-processing
            switch (fileType)
            {
                case "ltx":
                    result = result.Replace("\"", "'");
                    if (Regex.Match(result, @"\s").Success)
                        result = "\"" + result + "\"";
                    break;
                case "xml":
                    result = SecurityElement.Escape(result);
                    break;
                case "script":
                    result = Regex.Replace(result, @"(?<!\\)""", @"\""");
                    break;
            }

            return left + result + right;
        }

        private static string Split(string line)
        {
            if (!anyCyrillic.Match(line).Success)
                return line;

            if (CheckCorpus(line))
                return GetCorpus(line);

            foreach (Regex pattern in splitPoints)
            {
                Match match = pattern.Match(line);
                if (match.Success)
                    return Split(line.Substring(0, match.Index)) + match.Value + Split(line.Substring(match.Index + match.Length));
            }

            fromCorpus = false;
            return MachineTranslate(line);
        }

        private static bool CheckCorpus(string input)
        {
            return customCorpus.ContainsKey(input) || staticCorpus.ContainsKey(input) || tempCorpus.ContainsKey(input);
        }

        private static string GetCorpus(string input)
        {
            if (customCorpus.ContainsKey(input))
                return customCorpus[input];
            else if (staticCorpus.ContainsKey(input))
                return staticCorpus[input];
            else if (tempCorpus.ContainsKey(input))
                return tempCorpus[input];
            translateForm.AddLog("Please don't call GetCorpus without asking CheckCorpus first.", currentFile);
            return input;
        }

        private static string AddSpace(Match line)
        {
            return ". " + line.Groups[1].Value;
        }

        private static string MachineTranslate(string toTranslate)
        {
            if (machineCache.ContainsKey(toTranslate))
                return machineCache[toTranslate];

            string sanitized = toTranslate;

            // Leading and trailing whitespace is stripped and added back later
            Match whitespace = whitespacePattern.Match(sanitized);
            sanitized = sanitized.Trim();

            // Google doesn't always handle these well
            sanitized = sanitized.Replace("«", "\"").Replace("»", "\"");

            // Add spaces after periods
            sanitized = dotCyrillic.Replace(sanitized, new MatchEvaluator(AddSpace));

            string result;
            string urlEncoded = HttpUtility.UrlEncode(sanitized);
            if (urlEncoded.Length <= 2044) // Use Google Translate
            {
                string url = "http://www.google.com/translate_t?oe=UTF-8&sl=ru&tl=en&text=" + urlEncoded;
                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                result = webClient.DownloadString(url);
                result = googleScrape.Match(result).Groups[1].Value;
                result = removeTags.Replace(result, "");
                result = HttpUtility.HtmlDecode(result);
            }
            else if (sanitized.Length <= 5000) // Use Microsoft Translator
            {
                string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?from=ru&to=en&text=" + urlEncoded;
                string authToken = "Bearer " + admAuth.GetAccessToken().access_token;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.Headers.Add("Authorization", authToken);
                httpWebRequest.Timeout = 10000;
                WebResponse response;
                try
                {
                    response = httpWebRequest.GetResponse();
                    using (Stream stream = response.GetResponseStream())
                    {
                        DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                        result = (string)dcs.ReadObject(stream);
                    }
                    response.Close();
                }
                catch
                {
                    //result = sanitized;
                    ///*
                    using (TranslationInterruptForm form = new TranslationInterruptForm("MS Translator failed response.", sanitized))
                        form.ShowDialog();
                    result = InterruptResult;
                    //*/
                }
            }
            else
            {
                using (TranslationInterruptForm form = new TranslationInterruptForm("String too long to auto-translate.", sanitized))
                    form.ShowDialog();
                result = InterruptResult;
            }
            result = whitespace.Groups[1].Value + result + whitespace.Groups[2].Value;
            machineCache[toTranslate] = result;
            return result;
        }

        private static void LoadCorpus(Dictionary<string, string> target, string source)
        {
            XmlDocument corpusXML = new XmlDocument();
            corpusXML.Load("corpus\\" + source);
            foreach (XmlNode node in corpusXML.DocumentElement.ChildNodes)
                target[node.ChildNodes[0].InnerText] = node.ChildNodes[1].InnerText;
            translateForm.SetStatus("Loaded " +
                corpusXML.DocumentElement.ChildNodes.Count
                + " string pairs from " + source);
        }
    }
}
