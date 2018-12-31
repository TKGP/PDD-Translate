using System.Collections.Concurrent;
using System.Web;
using System.Xml;

namespace PDD_Translate_Automatic
{
    using Corpus = ConcurrentDictionary<string, ConcurrentDictionary<string, string>>;

    class PDDCorpus
    {
        private static Corpus modCorpus = new Corpus(), customCorpus = new Corpus(),
            socCorpus = new Corpus(), csCorpus = new Corpus(), copCorpus = new Corpus(), cocCorpus = new Corpus();

        public static void Init()
        {
            LoadCorpus(modCorpus, "sgm 2.2"); // By nashathedog
            LoadCorpus(modCorpus, "ogse 0692");
            LoadCorpus(modCorpus, "amk retranslated"); // By Conor Finegan
            LoadCorpus(modCorpus, "arena extension mod"); // By Siro and Darius6
            LoadCorpus(modCorpus, "lost alpha");
            LoadCorpus(cocCorpus, "call of chernobyl");
            LoadCorpus(socCorpus, "shadow of chernobyl");
            LoadCorpus(csCorpus, "clear sky");
            LoadCorpus(copCorpus, "call of pripyat");
            LoadCorpus(customCorpus, "custom");
        }

        private static void LoadCorpus(Corpus corpus, string source)
        {
            XmlDocument corpusXML = new XmlDocument();
            corpusXML.Load(@"res\corpus\" + source + ".xml");
            foreach (XmlNode textNode in corpusXML.DocumentElement.ChildNodes)
            {
                if (textNode.ChildNodes.Count == 1)
                    //ProgressForm.Bug("Lonely node found in corpus :(");
                    continue;
                else if (textNode.SelectSingleNode("ru") == null)
                    //ProgressForm.Bug("Rus missing from node");
                    continue;
                else
                {
                    string ru = HttpUtility.HtmlDecode(textNode.SelectSingleNode("ru").InnerText);
                    if (!corpus.ContainsKey(ru))
                        corpus[ru] = new ConcurrentDictionary<string, string>();
                    foreach (XmlNode langNode in textNode.ChildNodes)
                        if (langNode.Name != "ru")
                            corpus[ru][langNode.Name] = HttpUtility.HtmlDecode(langNode.InnerText);
                }
            }
        }

        private static bool GetFromSpecificCorpus(Corpus corpus, string text, out string result)
        {
            if (corpus.ContainsKey(text) && corpus[text].ContainsKey(PDDLanguage.Current.GoogleCode))
            {
                result = corpus[text][PDDLanguage.Current.GoogleCode];
                return true;
            }
            result = null;
            return false;
        }

        public static bool GetFromCorpus(string text, out string result)
        {
            if (GetFromSpecificCorpus(customCorpus, text, out result))
                return true;
            // Surely there's a better way to do this
            switch (PDDOptions.Game)
            {
                case PDDOptions.Games.SoC:
                    if (GetFromSpecificCorpus(socCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(copCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(csCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(cocCorpus, text, out result))
                        return true;
                    break;
                case PDDOptions.Games.CS:
                    if (GetFromSpecificCorpus(csCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(copCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(socCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(cocCorpus, text, out result))
                        return true;
                    break;
                case PDDOptions.Games.CoP:
                    if (GetFromSpecificCorpus(copCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(csCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(socCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(cocCorpus, text, out result))
                        return true;
                    break;
                case PDDOptions.Games.CoC:
                    if (GetFromSpecificCorpus(cocCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(copCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(csCorpus, text, out result))
                        return true;
                    if (GetFromSpecificCorpus(socCorpus, text, out result))
                        return true;
                    break;
            }
            if (GetFromSpecificCorpus(modCorpus, text, out result))
                return true;
            return false;
        }
    }
}
