using System.Collections.Generic;
using System.Text;

namespace PDD_Translate_Automatic
{
    class PDDLanguage
    {
        public string Name;
        public Encoding Encoding;
        private string stalkerCode;
        public string StalkerCode
        {
            get
            {
                // Horrible, I know
                if (PDDOptions.Game == PDDOptions.Games.SoC && stalkerCode == "spa")
                    return "esp";
                else
                    return stalkerCode;
            }
            set { stalkerCode = value; }
        }
        // It's the same for Microsoft Translator
        public string GoogleCode;
        // Because GSC doesn't know what a suffix is
        public string FontPrefix;

        public PDDLanguage(string name, int encoding, string stalkerCode, string googleCode, string fontPrefix)
        {
            Name = name;
            if (encoding == 1252)
                Encoding = Encoding.GetEncoding(encoding, new CyrillicToRomanFallback(), new DecoderExceptionFallback());
            else
                Encoding = Encoding.GetEncoding(encoding);
            StalkerCode = stalkerCode;
            GoogleCode = googleCode;
            FontPrefix = fontPrefix;
        }

        public static List<PDDLanguage> Languages = new List<PDDLanguage>()
        {
            // 1252 is the standard but since it doesn't matter for EN and breaks RU comments this is fine
            new PDDLanguage("English", 1251, "eng", "en", ""),
            new PDDLanguage("French", 1252, "fra", "fr", "_west"),
            new PDDLanguage("German", 1252, "ger", "de", "_west"),
            new PDDLanguage("Italian", 1252, "ita", "it", "_west"),
            //new PDDLanguage("Russian", 1251, "rus", "ru", ""),
            // esp in SoC, but that's kludged above
            new PDDLanguage("Spanish", 1252, "spa", "es", "_west"),
        };
        public static PDDLanguage Russian = new PDDLanguage("Russian", 1251, "rus", "ru", "");
        public static PDDLanguage Current;
    }
}
