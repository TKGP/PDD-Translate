using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PDDTranslate
{
    static class Program
    {
        //[STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DialogResult result = new OptionsForm().ShowDialog();
            if (result == DialogResult.OK)
            {
                TranslateForm translateForm = new TranslateForm();
                Translator.SetParentGUI(translateForm);
                Translator.Start();
                translateForm.ShowDialog();
            }
        }
    }
}
