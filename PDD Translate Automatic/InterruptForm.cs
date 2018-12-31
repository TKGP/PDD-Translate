using System;
using System.Windows.Forms;

namespace PDD_Translate_Automatic
{
    public partial class InterruptForm : Form
    {
        private static object interruptLock = new object();
        private static string interruptResult;

        public static string GetInterrupt(string input, string lang)
        {
            lock (interruptLock)
            {
                new InterruptForm(input, lang).ShowDialog();
                return interruptResult;
            }
        }

        public InterruptForm(string input, string lang)
        {
            InitializeComponent();
            label1.Text = "(" + lang + ")";
            textBox1.Text = input;
            Clipboard.SetText(input);
            textBox2.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            interruptResult = textBox2.Text;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }
    }
}
