using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace PDD_Translate_Automatic
{
    public partial class ProgressForm : Form
    {
        private static ProgressForm current;
        private Stopwatch stopwatch = new Stopwatch();

        private static object googleLock = new object();
        private static object corpusLock = new object();
        private static object logLock = new object();
        private static object bugLock = new object();
        private int googleChars = 0, corpusChars = 0, filesModded = 0;
        private StringBuilder logSb = new StringBuilder(), bugSb = new StringBuilder();

        public ProgressForm()
        {
            current = this;
            InitializeComponent();
        }

        private void InvokeThis(Action toInvoke)
        {
            this.Invoke(toInvoke);
        }

        public static void Log(string message)
        {
            lock (logLock)
            {
                if (current.logSb.Length > 0)
                    current.logSb.AppendLine();
                current.logSb.Append(message);
            }
        }

        public static void Bug(string message)
        {
            lock (bugLock)
            {
                if (current.bugSb.Length > 0)
                    current.bugSb.AppendLine();
                current.bugSb.Append(message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        public void Start()
        {
            stopwatch.Restart();
        }

        public void Finish()
        {
            stopwatch.Stop();
            InvokeThis(() => button1.Enabled = true);
        }

        private void SetProgressMax(ProgressBar progressBar, Label label, int max)
        {
            InvokeThis(() =>
            {
                progressBar.Maximum = max + 1;
                progressBar.Value = 1;
            });
        }
        private void AddProgress(ProgressBar progressBar, Label label)
        {
            InvokeThis(() =>
            {
                progressBar.Value += 1;
            });
        }

        public void SetScriptMax(int max)
        {
            SetProgressMax(progressBar1, label2, max);
        }

        public void AddScriptProgress()
        {
            AddProgress(progressBar1, label2);
        }

        public void SetXmlMax(int max)
        {
            SetProgressMax(progressBar2, label3, max);
        }

        public void AddXmlProgress()
        {
            AddProgress(progressBar2, label3);
        }

        public void SetLtxMax(int max)
        {
            SetProgressMax(progressBar3, label5, max);
        }

        public void AddLtxProgress()
        {
            AddProgress(progressBar3, label5);
        }

        public void SetStringMax(int max)
        {
            SetProgressMax(progressBar4, label7, max);
        }

        public void AddStringProgress()
        {
            AddProgress(progressBar4, label7);
        }

        public static void AddGoogle(int chars)
        {
            lock (googleLock)
                current.googleChars += chars;
        }

        public static void AddCorpus(int chars)
        {
            lock (corpusLock)
                current.corpusChars += chars;
        }

        public static void AddFilesModded()
        {
            current.filesModded++;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label13.Text = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            label14.Text = String.Format("{0:n0}", googleChars);
            label15.Text = String.Format("{0:n0}", corpusChars);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            lock (logLock)
                textBox1.Text = logSb.ToString();
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
            lock (bugLock)
                textBox2.Text = bugSb.ToString();
            textBox2.SelectionStart = textBox2.Text.Length;
            textBox2.ScrollToCaret();

            label2.Text = (progressBar1.Value - 1) + "/" + (progressBar1.Maximum - 1);
            label3.Text = (progressBar2.Value - 1) + "/" + (progressBar2.Maximum - 1);
            label5.Text = (progressBar3.Value - 1) + "/" + (progressBar3.Maximum - 1);
            label7.Text = (progressBar4.Value - 1) + "/" + (progressBar4.Maximum - 1);

            label17.Text = filesModded.ToString();
        }

        public string EditReadme(string readme)
        {
            readme = readme.Replace("$files", filesModded.ToString());
            readme = readme.Replace("$google", String.Format("{0:n0}", googleChars));
            readme = readme.Replace("$corpus", String.Format("{0:n0}", corpusChars));
            if (stopwatch.Elapsed.Hours > 0)
                readme = readme.Replace("$time", stopwatch.Elapsed.ToString("h'h 'm'm 's's'"));
            else if (stopwatch.Elapsed.Minutes > 0)
                readme = readme.Replace("$time", stopwatch.Elapsed.ToString("m'm 's's'"));
            else
                readme = readme.Replace("$time", stopwatch.Elapsed.ToString("s's'"));
            return readme;
        }
    }
}
