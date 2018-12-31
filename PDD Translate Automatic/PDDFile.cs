using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PDD_Translate_Automatic
{
    class PDDFile
    {
        private string filePath, oldText, fileType;
        private Dictionary<string, string> newText = new Dictionary<string, string>();
        private PDDComments comments;
        private BlockingCollection<Match> matchQueue;
        private ConcurrentDictionary<int, string> translationsByIndex;

        public PDDFile(string setFilePath)
        {
            filePath = setFilePath;
            oldText = File.ReadAllText(filePath, PDDLanguage.Russian.Encoding);
            foreach (PDDLanguage language in PDDOptions.Languages)
                newText[language.GoogleCode] = string.Copy(oldText);
            fileType = Path.GetExtension(filePath);
            comments = new PDDComments(fileType, oldText);
        }

        public void ProcessPattern(Regex pattern)
        {
            foreach (PDDLanguage language in PDDOptions.Languages)
            {
                PDDLanguage.Current = language;
                matchQueue = new BlockingCollection<Match>();
                translationsByIndex = new ConcurrentDictionary<int, string>();
                // Please optimize this later
                List<Thread> workers = new List<Thread>();
                for (int n = 1; n <= PDDOptions.Threads; n++)
                {
                    Thread worker = new Thread(() => ProcessMatches());
                    worker.IsBackground = true;
                    worker.SetApartmentState(ApartmentState.STA);
                    workers.Add(worker);
                    worker.Start();
                }
                foreach (Match match in pattern.Matches(newText[language.GoogleCode]))
                    matchQueue.Add(match);
                matchQueue.CompleteAdding();
                foreach (Thread worker in workers)
                    worker.Join();
                newText[language.GoogleCode] = pattern.Replace(newText[language.GoogleCode], GetReplacement);
            }
        }

        private void ProcessMatches()
        {
            foreach (Match match in matchQueue.GetConsumingEnumerable())
            {
                if (!comments.IsMatchCommented(match))
                {
                    string translation = PDDText.TranslateMatch(match, fileType);
                    if (translation != null)
                        translationsByIndex[match.Index] = translation;
                }
            }
        }

        private string GetReplacement(Match match)
        {
            if (translationsByIndex.ContainsKey(match.Index))
                return translationsByIndex[match.Index];
            else
                return match.Value;
        }

        public void Write(string outputDir)
        {
            bool addOnce = false;
            foreach (PDDLanguage language in PDDOptions.Languages)
            {
                string gamedata;
                if (PDDOptions.GenDistribution)
                    gamedata = outputDir + @"\" + language.Name + @"\gamedata\";
                else
                    gamedata = outputDir + @"\";
                if (newText[language.GoogleCode] != oldText)
                {
                    if (!addOnce)
                    {
                        addOnce = true;
                        ProgressForm.AddFilesModded();
                    }
                    string outPath = gamedata + filePath.Substring(PDDOptions.InputDir.Length);
                    new FileInfo(outPath).Directory.Create();
                    bool finished;
                    do
                    {
                        try
                        {
                            File.WriteAllText(outPath, newText[language.GoogleCode], language.Encoding);
                            finished = true;
                        }
                        catch (IOException)
                        {
                            DialogResult result = MessageBox.Show("File write failed due to Read Only status. Please unmark Read Only and press Retry, or Cancel to skip.\r\n" + outPath,
                                "Failure to Write File", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                            if (result == DialogResult.Retry)
                                finished = false;
                            else
                                finished = true;
                        }
                    } while (!finished);
                }
            }
        }
    }
}
