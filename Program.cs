using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PDDTranslate
{
    static class Program
    {
        public static bool quit = true;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OptionsForm());
            if (!quit)
                Application.Run(new TranslateForm());
        }
    }
}
