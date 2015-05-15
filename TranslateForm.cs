﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;

namespace PDDTranslate
{
    public partial class TranslateForm : Form
    {
        readonly Encoding RUENCODING = Encoding.GetEncoding(1251);

        static AdmAuthentication admAuth;

        public static string iniCheck, scriptCheck, xmlCheck, stringCheck;
        bool skipString, skipCorpus;

        string currentMode, fileType; // Barf
        bool corpusOnly;
        MatchCollection comments, moreComments;
        int numLogs = 0;

        static EventWaitHandle waitHandle = new AutoResetEvent(false);
        Thread fileParser;

        Dictionary<string, string> vanillaCorpus = new Dictionary<string, string>();
        Dictionary<string, string> customCorpus = new Dictionary<string, string>();
        Dictionary<string, string> tempCorpus = new Dictionary<string, string>();
        Dictionary<string, string> machineCache = new Dictionary<string, string>();
        XmlDocument customCorpusXML = new XmlDocument();

        Dictionary<string, string> doneFiles = new Dictionary<string, string>();

        Regex anyCyrillic = new Regex("[АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ]", RegexOptions.IgnoreCase); // Ё
        Regex whitespacePattern = new Regex(@"^(\s*).+?(\s*)$");
        Regex dotCyrillic = new Regex(@"\.([АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ])");
        Regex googleScrape = new Regex("<span[^>]+?result_box[^>]*?>(.+?)</span></div>");
        Regex removeTags = new Regex("<.+?>(?!\")");

        Regex scriptText = new Regex(@"""(([^""\n]|\\"")*?)(?<!\\)""");
        Regex itemText = new Regex(@"((inv_name|description).*?=\s*)([^;\n]+[^;\s])");
        Regex textText = new Regex("(<text[^>]*?>)(.*?)</text>", RegexOptions.Singleline);
        Regex attributeText = new Regex("(<[^>]*?(hint|name|caption)=\")([^\"]*?)\"");
        Regex gameplayText = new Regex(@"<(name|title|text)>([^<]*?)</\1>");

        Regex[] splitPoints = {
            new Regex(@"\. "),
            new Regex(@"\n"),
            new Regex(@"\\?\\n"),
            new Regex(@"\$\$.+?\$\$"), // Keybind macro
            new Regex(@"%c\[[^]]+?]"), // Color code
            new Regex(@"%(?:\d+\$)?[dfsu]"), // string.format arguments
            new Regex(@"\$\S+") // AMK-style placeholders
            };

        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll")]
        static extern int GetScrollPos(IntPtr hWnd, int nBar);

        // Form functions
        public TranslateForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string token = File.ReadAllText("token.txt");
            admAuth = new AdmAuthentication("PDDTranslate", token);

            SetStatus("Loading corpora...");
            XmlDocument vanillaCorpusXML = new XmlDocument();
            vanillaCorpusXML.Load("vanilla_corpus.xml");
            foreach (XmlNode node in vanillaCorpusXML.DocumentElement.ChildNodes)
                vanillaCorpus[node.ChildNodes[0].InnerText] = node.ChildNodes[1].InnerText;
            SetStatus("Loaded " + vanillaCorpus.Count + " string pairs from vanilla corpus.");
            customCorpusXML.Load("custom_corpus.xml");
            foreach (XmlNode node in customCorpusXML.DocumentElement.ChildNodes)
                customCorpus[node.ChildNodes[0].InnerText] = node.ChildNodes[1].InnerText;
            SetStatus("Loaded " + customCorpus.Count + " string pairs from custom corpus.");

            SetStatus("Starting file parsing thread...");
            fileParser = new Thread(new ThreadStart(ParseFiles));
            fileParser.SetApartmentState(ApartmentState.STA);
            fileParser.IsBackground = true;
            fileParser.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            waitHandle.Set();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            skipString = true;
            waitHandle.Set();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            skipCorpus = true;
            waitHandle.Set();
        }

        // File parsing functions
        void ParseFiles()
        {
            if (Directory.Exists("output"))
                Directory.Delete("output", true);
            Directory.CreateDirectory("output");
            string configPath = Directory.Exists(@"input\config") ? "config" : "configs";
            int howMany;

            SetStatus((scriptCheck != "skip" ? "Checking" : "Skipping") + " scripts...\t\t(1/5)");
            howMany = IterateFiles(scriptCheck, "scripts", "*.script", scriptText, new MatchEvaluator(GetScriptTranslation));
            if (howMany > -1) SetStatus("Translated " + howMany + " files.");

            SetStatus((iniCheck != "skip" ? "Checking" : "Skipping") + " items...\t\t(2/5)");
            howMany = IterateFiles(iniCheck, configPath, "*.ltx", itemText, new MatchEvaluator(GetItemTranslation));
            if (howMany > -1) SetStatus("Translated " + howMany + " files.");

            SetStatus((xmlCheck != "skip" ? "Checking" : "Skipping") + " \\gameplay xml...\t(3/5)");
            howMany = IterateFiles(xmlCheck, configPath + @"\gameplay", "*.xml", gameplayText, new MatchEvaluator(GetGplayTranslation));
            howMany += IterateFiles(xmlCheck, configPath + @"\gameplay", "*.xml", attributeText, new MatchEvaluator(GetAttributeTranslation));
            if (howMany > -1) SetStatus("Translated " + howMany + " files.");

            SetStatus((xmlCheck != "skip" ? "Checking" : "Skipping") + " \\ui xml...\t\t(4/5)");
            howMany = IterateFiles(xmlCheck, configPath + @"\ui", "*.xml", textText, new MatchEvaluator(GetTextTranslation));
            howMany += IterateFiles(xmlCheck, configPath + @"\ui", "*.xml", attributeText, new MatchEvaluator(GetAttributeTranslation));
            if (howMany > -1) SetStatus("Translated " + howMany + " files.");

            SetStatus((stringCheck != "skip" ? "Checking" : "Skipping") + " strings...\t\t(5/5)");
            howMany = IterateFiles(stringCheck, configPath + @"\text\rus", "*.xml", textText, new MatchEvaluator(GetTextTranslation));
            if (howMany > -1) SetStatus("Translated " + howMany + " files.");

            EnableControls(false);
            SetStatus("Done!");
            SystemSounds.Asterisk.Play();
        }

        int IterateFiles(string mode, string path, string filePattern, Regex pattern, MatchEvaluator translateFunc)
        {
            if (mode == "skip")
                return -1;
            if (mode == "auto")
                EnableControls(false);
            else
                EnableControls(true);
            if (!Directory.Exists(@"input\" + path))
            {
                SetStatus(@"Directory input\" + path + " not found, skipping...");
                return -1;
            }
            int fileCount = 0;
            string[] filePaths = Directory.GetFiles(@"input\" + path + @"\", filePattern, SearchOption.AllDirectories);
            Directory.CreateDirectory(@"output\" + path);
            currentMode = mode;
            int howMany = 0;
            foreach (string filePath in filePaths)
            {
                fileType = Regex.Match(filePath, @".+\.(.+)").Groups[1].Value.ToLower();
                string fileText, newText;
                fileCount++;
                SetLabel(label6, filePath + " (" + fileCount + "/" + filePaths.Length + ")");
                if (doneFiles.ContainsKey(filePath))
                    fileText = doneFiles[filePath];
                else
                    fileText = File.ReadAllText(filePath, RUENCODING);
                SetTextBox(textBox3, fileText);
                moreComments = null;
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
                        throw null; // :^)
                }

                newText = pattern.Replace(fileText, translateFunc);
                if (newText != fileText)
                {
                    howMany++;
                    doneFiles[filePath] = newText;
                    string where = @"output\" + Regex.Match(filePath.Substring(5), @".+\\");
                    Directory.CreateDirectory(where);
                    File.WriteAllText(@"output\" + filePath.Substring(5), newText, RUENCODING);
                    customCorpusXML.Save("custom_corpus.xml");
                }
            }
            return howMany;
        }

        string GetTranslation(string line, int index, int length)
        {
            if (!anyCyrillic.Match(line).Success)
                return line;

            ScrollTo(textBox3, index, length);

            int endIndex = index + length;
            foreach (Match comment in comments)
            {
                if ((index >= comment.Index && index <= comment.Index + comment.Length)
                    || (endIndex >= comment.Index && endIndex <= comment.Index + comment.Length))
                {
                    return line;
                }
                else if (comment.Index >= index && comment.Index <= endIndex)
                    SetLog("Comment inside string?!");
            }
            if (moreComments != null)
            {
                foreach (Match comment in moreComments)
                {
                    if ((index >= comment.Index && index <= comment.Index + comment.Length)
                        || (endIndex >= comment.Index && endIndex <= comment.Index + comment.Length))
                    {
                        return line;
                    }
                    else if (comment.Index >= index && comment.Index <= endIndex)
                        SetLog("Comment inside string?!");
                }
            }

            string newLine = line;

            // Pre-processing
            switch (fileType)
            {
                case "ltx":
                    if (newLine.IndexOf("\"") == 0)
                        newLine = newLine.Substring(1).Substring(0, newLine.Length - 1);
                    break;
                case "xml":
                    newLine = HttpUtility.HtmlDecode(newLine);
                    break;
                case "script":
                    newLine = newLine.Replace("\\\"", "\"");
                    break;
            }

            string result = null;
            if (CheckCorpus(newLine))
                result = GetCorpus(newLine);
            if (result != null && currentMode == "semi")
                return result;

            if (result == null)
            {
                corpusOnly = true;
                result = Split(newLine);
                if (corpusOnly && currentMode == "semi")
                    return result;
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

            if (currentMode == "auto")
                return result;

            SetTextBox(textBox1, line);
            SetTextBox(textBox2, result);
            Clipboard.SetText(line);
            this.Invoke(new Action(() => textBox2.Focus()));

            waitHandle.WaitOne(); // Wait for user input

            if (skipString)
            {
                skipString = false;
                return line;
            }

            if (!skipCorpus)
            {
                customCorpus[line] = textBox2.Text;
                XmlNode corpusNode = customCorpusXML.DocumentElement.AppendChild(customCorpusXML.CreateElement("text"));
                corpusNode.AppendChild(customCorpusXML.CreateElement("rus")).InnerText = line;
                corpusNode.AppendChild(customCorpusXML.CreateElement("eng")).InnerText = textBox2.Text;
            }
            else
            {
                skipCorpus = false;
                tempCorpus[line] = textBox2.Text;
            }

            return textBox2.Text;
        }

        string Split(string line)
        {
            if (!anyCyrillic.Match(line).Success)
                return line;

            foreach (Regex pattern in splitPoints)
            {
                Match match = pattern.Match(line);
                if (match.Success)
                    return Split(line.Substring(0, match.Index)) + match.Value + Split(line.Substring(match.Index + match.Length));
            }

            if (CheckCorpus(line))
                return GetCorpus(line);
            else
            {
                corpusOnly = false;
                return MachineTranslate(line);
            }
        }

        bool CheckCorpus(string input)
        {
            return customCorpus.ContainsKey(input) || vanillaCorpus.ContainsKey(input) || tempCorpus.ContainsKey(input);
        }

        string GetCorpus(string input)
        {
            if (customCorpus.ContainsKey(input))
                return customCorpus[input];
            else if (vanillaCorpus.ContainsKey(input))
                return vanillaCorpus[input];
            else if (tempCorpus.ContainsKey(input))
                return tempCorpus[input];
            SetLog("Please don't call GetCorpus without asking CheckCorpus first.");
            return input;
        }

        string AddSpace(Match line)
        {
            return ". " + line.Groups[1].Value;
        }

        string GetScriptTranslation(Match line)
        {
            return "\"" + GetTranslation(line.Groups[1].Value, line.Groups[1].Index, line.Groups[1].Length) + "\"";
        }
        string GetItemTranslation(Match line)
        {
            return line.Groups[1].Value + GetTranslation(line.Groups[3].Value, line.Groups[3].Index, line.Groups[3].Length);
        }
        string GetGplayTranslation(Match line)
        {
            return "<" + line.Groups[1].Value + ">" + GetTranslation(line.Groups[2].Value, line.Groups[2].Index, line.Groups[2].Length) + "</" + line.Groups[1].Value + ">";
        }
        string GetAttributeTranslation(Match line)
        {
            return line.Groups[1].Value + GetTranslation(line.Groups[3].Value, line.Groups[3].Index, line.Groups[3].Length) + "\"";
        }
        string GetTextTranslation(Match line)
        {
            return line.Groups[1].Value + GetTranslation(line.Groups[2].Value, line.Groups[2].Index, line.Groups[2].Length) + "</text>";
        }

        // Form access functions
        void EnableControls(bool value)
        {
            this.Invoke(new Action(() =>
            {
                button1.Enabled = value;
                button2.Enabled = value;
                button3.Enabled = value;
                if (!value)
                {
                    textBox1.Clear();
                    textBox2.Clear();
                }
            }));
        }
        void ScrollTo(TextBox target, int index, int length)
        {
            this.Invoke(new Action(() =>
            {
                target.Focus();
                int scroll = target.GetLineFromCharIndex(index) - GetScrollPos(target.Handle, 1);
                SetScrollPos(target.Handle, 1, scroll, true);
                SendMessage(target.Handle, 0x00B6, 0, scroll);
                target.Select(index, length);
            }));
        }
        void SetLabel(Label target, string content)
        {
            this.Invoke(new Action(() => target.Text = content));
        }
        void SetTextBox(TextBox target, string content)
        {
            this.Invoke(new Action(() => target.Text = content));
        }
        void SetStatus(string status)
        {
            this.Invoke(new Action(() => textBox4.AppendText((textBox4.Lines.Length != 0 ? "\r\n" : "") + status)));
        }
        void SetLog(string log)
        {
            this.Invoke(new Action(() => textBox5.AppendText((textBox5.Lines.Length != 0 ? "\r\n" : "") +
                ++numLogs + ". " + Regex.Match(label6.Text, @"\\([^\\]+?) ").Groups[1].Value + ": " + log)));
        }

        // Get machine translation
        string MachineTranslate(string input)
        {
            if (input.Trim() == "")
                return input;

            if (machineCache.ContainsKey(input))
                return machineCache[input];

            // Leading and trailing whitespace is stripped and added back later
            Match whitespace = whitespacePattern.Match(input);

            // Google doesn't always handle these well
            input = input.Replace("«", "\"").Replace("»", "\"");

            // Add spaces after periods
            input = dotCyrillic.Replace(input, new MatchEvaluator(AddSpace));

            if (HttpUtility.UrlEncode(input).Length <= 2044) // Use Google Translate
            {
                string url = "http://www.google.com/translate_t?oe=UTF-8&sl=ru&tl=en&text=" + HttpUtility.UrlEncode(input.Trim());
                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                string result = webClient.DownloadString(url);
                result = googleScrape.Match(result).Groups[1].Value;
                result = removeTags.Replace(result, "");
                result = whitespace.Groups[1].Value + HttpUtility.HtmlDecode(result) + whitespace.Groups[2].Value;
                machineCache[input] = result;
                return result;
            }
            else if (input.Length <= 5000) // Use Microsoft Translator
            {
                string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?from=ru&to=en&text=" + HttpUtility.UrlEncode(input.Trim());
                string authToken = "Bearer " + admAuth.GetAccessToken().access_token;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.Headers.Add("Authorization", authToken);
                httpWebRequest.Timeout = 10000;
                WebResponse response;
                try
                {
                    response = httpWebRequest.GetResponse();
                }
                catch
                {
                    SetLog("MS Translator failed response.");
                    return input;
                }
                using (Stream stream = response.GetResponseStream())
                {
                    DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                    string result = whitespace.Groups[1].Value + (string)dcs.ReadObject(stream) + whitespace.Groups[2].Value;
                    machineCache[input] = result;
                    return result;
                }
            }
            else
            {
                SetLog("String too long to auto-translate.");
                return input;
            }
        }
    }
}
