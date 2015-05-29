

namespace PDDTranslate
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using ContinueOption = Translator.ContinueOption;
    public partial class TranslateForm : Form
    {
        private int numLogs = 0;

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll")]
        static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        public TranslateForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Translator.Continue(ContinueOption.ConfirmCorpus, textBox2.Text);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Translator.Continue(ContinueOption.Skip, textBox2.Text);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Translator.Continue(ContinueOption.ConfirmNoCorpus, textBox2.Text);
        }

        private void InvokeThis(Action toInvoke)
        {
            this.Invoke(toInvoke);
        }
        public void EnableControls()
        {
            InvokeThis(() =>
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                textBox2.Enabled = true;
            });
        }
        public void DisableControls()
        {
            InvokeThis(() =>
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                textBox1.Clear();
                textBox2.Clear();
                textBox2.Enabled = false;
                label1.Focus();
            });
        }
        public void ScrollFile(int index, int length)
        {
            InvokeThis(() =>
            {
                int scroll = textBox3.GetLineFromCharIndex(index) - GetScrollPos(textBox3.Handle, 1);
                SetScrollPos(textBox3.Handle, 1, scroll, true);
                SendMessage(textBox3.Handle, 0x00B6, 0, scroll);
                textBox3.Select(index, length);
            });
        }
        public void SetSuggestion(string russianText, string englishText)
        {
            InvokeThis(() =>
            {
                textBox1.Text = russianText;
                textBox2.Text = englishText;
                textBox2.Focus();
            });
        }
        public void SetFileText(string text)
        {
            InvokeThis(() => textBox3.Text = text);
        }
        public void SetCurrentFile(string text)
        {
            InvokeThis(() => label6.Text = text);
        }
        public void SetStatus(string status)
        {
            InvokeThis(() => textBox4.AppendText((textBox4.Lines.Length != 0 ? "\r\n" : "") + status));
        }
        public void AddLog(string log, string file)
        {
            InvokeThis(() => textBox5.AppendText((textBox5.Lines.Length != 0 ? "\r\n" : "") +
                ++numLogs + ". " + file + ": " + log));
        }
    }
}
