using System.Collections.Generic;

namespace PDD_Translate_Automatic
{
    class PDDOptions
    {
        public enum Games { SoC = 0, CS, CoP, CoC };
        public static Games Game;
        public static void SetGame(int index)
        {
            Game = (Games)index;
        }
        public static List<PDDLanguage> Languages;
        public static void SetLanguages(int index)
        {
            if (PDDOptions.GenDistribution)
                Languages = new List<PDDLanguage>(PDDLanguage.Languages);
            else
                Languages = new List<PDDLanguage>() { PDDLanguage.Languages[index] };
        }

        public static string InputDir, OutputDir;
        public static bool InPlace, BackupSource, DemoMode, ClearOutput, IncludeVanilla, IncludeLocal, GenDistribution;
        public static int Threads;
        public static string TitleEng, TitleRus, TitleShort, PatchVersion, IntendedUse, ModSite, DownloadLinks;
        public static bool DoStrings, DoXml, DoLtx, DoScripts, DoOther;
    }
}
