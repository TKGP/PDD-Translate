using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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
        const string RUSSIAN = "[АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ]";
        readonly Encoding RUENCODING = Encoding.GetEncoding(1251);

        static AdmAuthentication admAuth;

        public static string iniCheck, scriptCheck, xmlCheck, stringCheck;
        bool skipString, skipCorpus;

        string currentMode, fileType; // Barf

        static EventWaitHandle waitHandle = new AutoResetEvent(false);
        Thread fileParser;

        Dictionary<string, string> vanillaCorpus = new Dictionary<string, string>();
        Dictionary<string, string> customCorpus = new Dictionary<string, string>();
        Dictionary<string, string> tempCorpus = new Dictionary<string, string>();
        XmlDocument customCorpusXML = new XmlDocument();

        Dictionary<string, string> doneFiles = new Dictionary<string, string>();

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

            SetStatus((scriptCheck != "skip" ? "Checking" : "Skipping") + " scripts...\t\t(1/5)");
            IterateFiles(scriptCheck, "scripts", "*.script",
                "\"(([^\"\r\n]|\\\\\")*?" + RUSSIAN + "([^\"\r\n]|\\\\\")*?)\"",
                new MatchEvaluator(GetScriptTranslation));

            SetStatus((iniCheck != "skip" ? "Checking" : "Skipping") + " items...\t\t(2/5)");
            IterateFiles(iniCheck, configPath, "*.ltx",
                "((inv_name|description).*?=\\s*?)(\\S[^;\r\n]*?" + RUSSIAN + "[^;\r\n]*[^;\\s])",
                new MatchEvaluator(GetItemTranslation));

            SetStatus((xmlCheck != "skip" ? "Checking" : "Skipping") + " \\gameplay xml...\t(3/5)");
            IterateFiles(xmlCheck, configPath + @"\gameplay", "*.xml",
                "<(name|title|text)>([^<]*?" + RUSSIAN + @"[^<]*?)</\1>",
                new MatchEvaluator(GetGplayTranslation));
            IterateFiles(xmlCheck, configPath + @"\gameplay", "*.xml",
                "(<[^>]*?(hint|name)=\")([^\"]*?" + RUSSIAN + "[^\"]*?)\"",
                new MatchEvaluator(GetAttributeTranslation));

            SetStatus((xmlCheck != "skip" ? "Checking" : "Skipping") + " \\ui xml...\t\t(4/5)");
            IterateFiles(xmlCheck, configPath + @"\ui", "*.xml",
                "(<text[^>]*?>)([^<]*?" + RUSSIAN + "[^<]*?)</text>",
                new MatchEvaluator(GetTextTranslation));
            IterateFiles(xmlCheck, configPath + @"\ui", "*.xml",
                "(<[^>]*?(hint|name|caption)=\")([^\"]*?" + RUSSIAN + "[^\"]*?)\"",
                new MatchEvaluator(GetAttributeTranslation));

            SetStatus((stringCheck != "skip" ? "Checking" : "Skipping") + " strings...\t\t(5/5)");
            IterateFiles(stringCheck, configPath + @"\text\rus", "*.xml",
                "(<text[^>]*?>)([^<]*?" + RUSSIAN + "[^<]*?)</text>",
                new MatchEvaluator(GetTextTranslation));

            setButtonEnable(false);
            SetStatus("Done!");
            SystemSounds.Asterisk.Play();
        }

        void IterateFiles(string mode, string path, string filePattern, string regexPattern, MatchEvaluator translateFunc)
        {
            if (mode == "skip")
                return;
            if (mode == "auto")
                setButtonEnable(false);
            else
                setButtonEnable(true);
            if (!Directory.Exists(@"input\" + path))
            {
                SetStatus(@"Directory input\" + path + " not found, skipping...");
                return;
            }
            int fileCount = 0;
            string[] filePaths = Directory.GetFiles(@"input\" + path + @"\", filePattern, SearchOption.AllDirectories);
            Directory.CreateDirectory(@"output\" + path);
            currentMode = mode;
            foreach (string filePath in filePaths)
            {
                fileType = Regex.Match(filePath, @"\.(.+)").Groups[1].Value;
                string fileText, newText;
                fileCount++;
                SetLabel(label6, filePath + " (" + fileCount + "/" + filePaths.Length + ")");
                if (doneFiles.ContainsKey(filePath))
                    fileText = doneFiles[filePath];
                else
                    fileText = File.ReadAllText(filePath, RUENCODING);
                SetTextBox(textBox3, fileText);
                newText = Regex.Replace(fileText, regexPattern, translateFunc, RegexOptions.IgnoreCase);
                if (newText != fileText)
                {
                    doneFiles[filePath] = newText;
                    string where = @"output\" + Regex.Match(filePath.Substring(5), @".+\\");
                    Directory.CreateDirectory(where);
                    File.WriteAllText(@"output\" + filePath.Substring(5), newText, RUENCODING);
                    customCorpusXML.Save("custom_corpus.xml");
                }
            }
        }

        string Split(string line)
        {
            if (line.Contains(@"\n"))
            {
                int index = line.IndexOf(@"\n");
                return Split(line.Substring(0, index)) + @"\n" + Split(line.Substring(index + 2));
            }
            else if (line.Contains("\n"))
            {
                int index = line.IndexOf("\n");
                return Split(line.Substring(0, index)) + "\n" + Split(line.Substring(index + 1));
            }
            else if (line.Contains("\\\""))
            {
                int index = line.IndexOf("\\\"");
                return Split(line.Substring(0, index)) + "\\\"" + Split(line.Substring(index + 2));
            }
            else if (line.Contains("%c["))
            {
                Match match = Regex.Match(line, @"%c\[[^]]+?]");
                if (!match.Success)
                {
                    SetLog("Incomplete color tag in " + Regex.Match(label6.Text, @"\\([^\\]+?) ").Groups[1].Value);
                    return line;
                }
                else
                {
                    int index = match.Index;
                    int length = match.Length;
                    return Split(line.Substring(0, index)) + match.Value + Split(line.Substring(index + length));
                }
            }
            else if (CheckCorpus(line))
                return GetCorpus(line);
            else
                return TranslateText(line);
        }

        string GetTranslation(string line)
        {
            string result = null;
            if (CheckCorpus(line))
                result = GetCorpus(line);
            if (result != null && currentMode == "semi")
                return result;

            if (result == null)
                result = Split(Regex.Replace(line, @"\.(" + RUSSIAN + ")", new MatchEvaluator(AddSpace)));

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
            ScrollTo(textBox3, line.Groups[1].Index, line.Groups[1].Length);
            return "\"" + GetTranslation(line.Groups[1].Value) + "\"";
        }
        string GetItemTranslation(Match line)
        {
            ScrollTo(textBox3, line.Groups[3].Index, line.Groups[3].Length);
            return line.Groups[1].Value + GetTranslation(line.Groups[3].Value);
        }
        string GetGplayTranslation(Match line)
        {
            ScrollTo(textBox3, line.Groups[2].Index, line.Groups[2].Length);
            return "<" + line.Groups[1].Value + ">" + GetTranslation(line.Groups[2].Value) + "</" + line.Groups[1].Value + ">";
        }
        string GetAttributeTranslation(Match line)
        {
            ScrollTo(textBox3, line.Groups[3].Index, line.Groups[3].Length);
            return line.Groups[1].Value + GetTranslation(line.Groups[3].Value) + "\"";
        }
        string GetTextTranslation(Match line)
        {
            ScrollTo(textBox3, line.Groups[2].Index, line.Groups[2].Length);
            return line.Groups[1].Value + GetTranslation(line.Groups[2].Value) + "</text>";
        }

        // Form access functions
        void setButtonEnable(bool value)
        {
            this.Invoke(new Action(() =>
            {
                button1.Enabled = value;
                button2.Enabled = value;
                button3.Enabled = value;
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
            this.Invoke(new Action(() => textBox5.AppendText((textBox5.Lines.Length != 0 ? "\r\n" : "") + (textBox5.Lines.Length + 1) + ". " + log)));
        }

        // Get machine translation
        string TranslateText(string input)
        {
            if (input == "" || input.Trim() == "")
            {
                return input;
            }

            string prefix = input[0] == ' ' ? " " : "";
            string suffix = input[input.Length - 1] == ' ' ? " " : "";
            if (HttpUtility.UrlEncode(input).Length <= 2044) // Use Google Translate
            {
                string url = "http://www.google.com/translate_t?oe=UTF-8&sl=ru&tl=en&text=" + HttpUtility.UrlEncode(input.Trim());
                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                string result = webClient.DownloadString(url);
                result = Regex.Match(result, "<span[^>]+?result_box[^>]*?>(.+?)</span></div>").Groups[1].Value;
                result = Regex.Replace(result, "<.+?>(?!\")", "");
                return prefix + HttpUtility.HtmlDecode(result) + suffix;
            }
            else if (input.Length <= 5000) // Use Microsoft Translator
            {
                //SetLog("String too long for Google, using MS.");
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
                    SetLog("MS Translator is probably out of characters.");
                    return input;
                }
                using (Stream stream = response.GetResponseStream())
                {
                    DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                    return prefix + (string)dcs.ReadObject(stream) + suffix;
                }
            }
            else
            {
                SetLog("Skipped string in " + Regex.Match(label6.Text, @"\\([^\\]+?) ").Groups[1].Value + ", too long to auto-translate.");
                return input;
            }
        }
    }
}
