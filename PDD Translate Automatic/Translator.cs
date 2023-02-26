using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.MyServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PDD_Translate_Automatic
{
    class Translator
    {
        private static Regex scriptRx = new Regex(@"([""'])(?<text>(?:[^\1\n]|\\\1)*?)(?<!\\)\1");
        private static Regex ltxRx = new Regex(@"(?:inv_name|inv_name_short|description|title|descr|hint|string)\s*?=\s*(?<text>[^;\n]+[^;\s])");
        private static Regex textRx = new Regex(@"<text[^>]*?>(?<text>[^<]*?)</text>");
        private static Regex attributeRx = new Regex("<[^>]*?(?:hint|name|caption)=\"(?<text>[^\"]*?)\"");
        private static Regex gameplayRx = new Regex(@"<(name|title)>(?<text>[^<]*?)</\1>");
        private static Regex amkVerRx = new Regex(@"(?:type|title|build|fix)\s*?=\s*(?<text>[^;\n]+[^;\s])");

        private static Regex fontPrefixRx = new Regex(@"^(\s*font_prefix\s*=)[^\r\n]*$", RegexOptions.Multiline);

        private static MainForm parentForm;
        private static ProgressForm progressForm;
        private static string outputDir;
        private static readonly string[] gameTextDir = { "soc", "cs", "cop", "coc" };
        private static readonly string[] shortMonths = { "Jan.", "Feb.", "Mar.", "Apr.", "May", "June", "July", "Aug.", "Sep.", "Oct.", "Nov.", "Dec." };

        public static void Translate(MainForm setParentForm, ProgressForm setProgressForm)
        {
            parentForm = setParentForm;
            progressForm = setProgressForm;

            PDDLanguage.Current = PDDOptions.Languages[0];
            // Convenient!
            string config = PDDOptions.Game == PDDOptions.Games.SoC ? "config" : "configs";
            string vanillaPath = @"res\vanilla\" + gameTextDir[(int)PDDOptions.Game] + @"\";
            FileSystemProxy vbFS = new Computer().FileSystem;

            if (PDDOptions.InPlace)
                outputDir = PDDOptions.InputDir;
            else if (PDDOptions.GenDistribution)
                outputDir = PDDOptions.OutputDir + @"\" + (PDDOptions.TitleShort != "" ? PDDOptions.TitleShort : PDDOptions.TitleEng) + " Language Pack " + PDDOptions.PatchVersion;
            else
                outputDir = PDDOptions.OutputDir + @"\gamedata";

            // Clear output directory
            if (PDDOptions.ClearOutput && !PDDOptions.InPlace)
            {
                if (Directory.Exists(outputDir))
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this directory?\r\n" + outputDir, "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            Directory.Delete(outputDir, true);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("Close Explorer and click OK you idiot");
                            Directory.Delete(outputDir, true);
                        }
                    }
                    else
                        return;
                }
            }

            progressForm.Start();

            // Backup source files
            if (PDDOptions.InPlace && PDDOptions.BackupSource)
            {
                if (Directory.Exists(PDDOptions.InputDir + @"\" + config))
                    vbFS.CopyDirectory(PDDOptions.InputDir + @"\" + config, PDDOptions.InputDir + @"\" + config + " backup");
                if (Directory.Exists(PDDOptions.InputDir + @"\scripts"))
                    vbFS.CopyDirectory(PDDOptions.InputDir + @"\scripts", PDDOptions.InputDir + @"\scripts backup");
            }

            Directory.CreateDirectory(outputDir);

            // Change language in localization.ltx and add fonts
            if (PDDOptions.IncludeLocal)
            {
                string localization;
                string localPath = PDDOptions.InputDir + @"\" + config + @"\localization.ltx";
                if (File.Exists(localPath))
                    localization = File.ReadAllText(localPath, PDDLanguage.Russian.Encoding);
                else
                    localization = File.ReadAllText(vanillaPath + "localization.ltx", PDDLanguage.Russian.Encoding);
                if (PDDOptions.GenDistribution)
                    vbFS.CopyDirectory(vanillaPath + "font _west", outputDir + @"\European Fonts\gamedata\textures\ui", true);
                foreach (PDDLanguage language in PDDOptions.Languages)
                {
                    Match match = fontPrefixRx.Match(localization);
                    string newLocal = fontPrefixRx.Replace(localization,match.Groups[1].Value + ' ' + language.FontPrefix);
                    string configPath;
                    if (PDDOptions.GenDistribution)
                        configPath = outputDir + @"\" + language.Name + @"\gamedata\" + config;
                    else
                        configPath = outputDir + @"\" + config;
                    Directory.CreateDirectory(configPath);
                    File.WriteAllText(configPath + @"\localization.ltx", newLocal, PDDLanguage.Russian.Encoding);
                }
            }

            // Translate!
            PDDText.GetStringIDs(PDDOptions.InputDir + @"\" + config);
            if (PDDOptions.DoScripts)
                ProcessFiles(GetFilesScripts(), scriptRx,
                    progressForm.SetScriptMax, progressForm.AddScriptProgress);
            if (PDDOptions.DoXml)
                ProcessFiles(GetFilesMiscXML(config), new Regex[] { textRx, gameplayRx, attributeRx },
                    progressForm.SetXmlMax, progressForm.AddXmlProgress);
            if (PDDOptions.DoLtx)
                ProcessFiles(GetFilesMiscLtx(config), ltxRx,
                    progressForm.SetLtxMax, progressForm.AddLtxProgress);
            if (PDDOptions.DoStrings)
                ProcessFiles(GetFilesStringTables(config), textRx,
                    progressForm.SetStringMax, progressForm.AddStringProgress);
            if (PDDOptions.DoOther)
            {
                // AlternativA and Way of Man: Return have the actor name here included from a string table for god knows what reason
                ProcessFiles(GetFilesSimple(config + @"\actor_name", ".xml"), textRx);
                // Return of Scar
                ProcessFiles(GetFilesAmkVer(), amkVerRx);
                // I forget what mod this is for :(
                //ProcessFiles(new string[] { configPath + @"\text\rus" }, ".ltx", progressForm.progressBar4, new Regex[] { new Regex(@"^(?!\[)(?<text>.+)", RegexOptions.Multiline) });
            }

            // Write readme
            if (PDDOptions.GenDistribution)
            {
                string readme = File.ReadAllText(@"res\readme template.txt");

                readme = readme.Replace("$eng_title", PDDOptions.TitleEng);
                readme = readme.Replace("$short_title", PDDOptions.TitleShort != "" ? PDDOptions.TitleShort : PDDOptions.TitleEng);
                readme = readme.Replace("$version", PDDOptions.PatchVersion);
                readme = readme.Replace("$mod_site", PDDOptions.ModSite);
                readme = readme.Replace("$download_links", PDDOptions.DownloadLinks);

                Match match = Regex.Match(readme, @"\$rus_title\[(.+?)\]");
                if (PDDOptions.TitleRus != "")
                    readme = readme.Replace(match.Value, match.Groups[1].Value.Replace("$rus_title", PDDOptions.TitleRus));
                else
                    readme = readme.Remove(match.Index, match.Length);
                match = Regex.Match(readme, @"\$intended\[(.+?)\]");
                if (PDDOptions.IntendedUse != "")
                    readme = readme.Replace(match.Value, match.Groups[1].Value.Replace("$intended", PDDOptions.IntendedUse));
                else
                    readme = readme.Remove(match.Index, match.Length);

                readme = readme.Replace("$config", config);
                //DateTime now = DateTime.Now;
                //string date = shortMonths[now.Month - 1] + " " + now.Day + ", " + now.Year;
                //readme = readme.Replace("$date", date);

                readme = progressForm.EditReadme(readme);

                File.WriteAllText(outputDir + @"\readme.txt", readme);
            }

            // Include vanilla string tables
            if (PDDOptions.IncludeVanilla)
            {
                foreach (PDDLanguage language in PDDOptions.Languages)
                {
                    if (!Directory.Exists(vanillaPath + language.StalkerCode))
                        continue;
                    string outputPath;
                    if (PDDOptions.GenDistribution)
                        outputPath = outputDir + @"\" + language.Name + @"\gamedata\" + config + @"\text\rus";
                    else
                        outputPath = outputDir + @"\" + config + @"\text\rus";
                    Directory.CreateDirectory(outputPath);
                    foreach (string stringTable in Directory.GetFiles(vanillaPath + language.StalkerCode))
                    {
                        string fileName = Path.GetFileName(stringTable);
                        if (!File.Exists(PDDOptions.InputDir + @"\" + config + @"\text\rus\" + fileName))
                            File.Copy(stringTable, outputPath + @"\" + fileName);
                    }
                }
            }

            SystemSounds.Asterisk.Play();
            progressForm.Finish();
            parentForm.Finish();
        }

        private static void ProcessFiles(List<string> filepaths, Regex pattern)
        {
            ProcessFiles(filepaths, new Regex[] { pattern }, null, null);
        }

        private static void ProcessFiles(List<string> filepaths, Regex pattern, Action<int> setMax, Action addProgress)
        {
            ProcessFiles(filepaths, new Regex[] { pattern }, setMax, addProgress);
        }

        private static void ProcessFiles(List<string> filepaths, Regex[] patterns, Action<int> setMax, Action addProgress)
        {
            setMax?.Invoke(filepaths.Count);
            foreach (string filepath in filepaths)
            {
                if (File.Exists(filepath))
                {
                    ProgressForm.Log("Processing: " + Regex.Match(filepath, @".+?((configs?|scripts)\\.+)").Groups[1].Value);
                    PDDFile file = new PDDFile(filepath);
                    foreach (Regex pattern in patterns)
                        file.ProcessPattern(pattern);
                    file.Write(outputDir);
                }
                else
                {
                    ProgressForm.Log("Missing: " + Regex.Match(filepath, @".+?((configs?|scripts)\\.+)").Groups[1].Value);
                }
                addProgress?.Invoke();
            }
        }

        private static List<string> GetFilesSimple(string subdir, string extension)
        {
            List<string> result = new List<string>();
            if (Directory.Exists(PDDOptions.InputDir + @"\" + subdir))
                foreach (string path in Directory.GetFiles(PDDOptions.InputDir + @"\" + subdir, "*" + extension, SearchOption.AllDirectories))
                    if (Path.GetExtension(path) == extension)
                        result.Add(path);
            return result;
        }

        private static List<string> GetFilesScripts()
        {
            return GetFilesSimple("scripts", ".script");
        }

        private static List<string> GetFilesMiscXML(string config)
        {
            return GetFilesSimple(config + @"\gameplay", ".xml")
                .Concat(GetFilesSimple(config + @"\ui", ".xml")).ToList();
        }

        private static List<string> GetFilesMiscLtx(string config)
        {
            return GetFilesSimple(config, ".ltx");
        }

        private static List<string> GetFilesStringTables(string config)
        {
            return GetFilesSimple(config + @"\text\rus", ".xml");
        }

        private static List<string> GetFilesAmkVer()
        {
            return new List<string>() {
               PDDOptions.InputDir + @"\config\shram.ltx",
               PDDOptions.InputDir + @"\config\narod_opt.ltx"
            };
        }
    }
}
