using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PDDTranslate
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TranslateForm.iniCheck = radioButton1.Checked ? "auto" : (radioButton2.Checked ? "semi" : (radioButton3.Checked ? "manual" : "skip"));
            TranslateForm.scriptCheck = radioButton6.Checked ? "auto" : (radioButton5.Checked ? "semi" : (radioButton4.Checked ? "manual" : "skip"));
            TranslateForm.xmlCheck = radioButton9.Checked ? "auto" : (radioButton8.Checked ? "semi" : (radioButton7.Checked ? "manual" : "skip"));
            TranslateForm.stringCheck = radioButton12.Checked ? "auto" : (radioButton11.Checked ? "semi" : (radioButton10.Checked ? "manual" : "skip"));
            Program.quit = false;
            Close();
        }
    }
}
