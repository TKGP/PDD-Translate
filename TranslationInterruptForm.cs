using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDDTranslate
{
    public partial class TranslationInterruptForm : Form
    {
        public TranslationInterruptForm(string reason, string text)
        {
            InitializeComponent();
            label1.Text = reason;
            textBox1.Text = text;
            Clipboard.SetText(text);
            textBox2.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Translator.SetInterruptResult(textBox2.Text);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Translator.SetInterruptResult(textBox1.Text);
            this.Close();
        }
    }
}
