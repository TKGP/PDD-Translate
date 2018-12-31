using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace PDD_Translate_Automatic
{
    public partial class MainForm : Form
    {
        private static RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software\PDDTranslate");
        private readonly Control[] mainControls, distributionOptions;
        private static Dictionary<Control, bool> controlStatus = new Dictionary<Control, bool>();

        public MainForm()
        {
            InitializeComponent();
            mainControls = new Control[] { buttonTranslate, checkBoxClear, checkBoxDemo, checkBoxScripts, checkBoxLtx, checkBoxXml, checkBoxStrings, checkBoxOther,
                checkBoxInPlace, checkBoxVanilla, checkBoxLocalization, comboBoxGame, comboBoxLanguage, groupBoxDistribution, textBoxInput, textBoxOutput };
            distributionOptions = new Control[] { buttonClear, textBoxTitleEng, textBoxTitleShort, textBoxTitleRus, textBoxVersion, textBoxSite, textBoxDownloads, textBoxIntention };

            textBoxInput.Text = (string)registry.GetValue("InputDir", "");
            checkBoxInPlace.Checked = (string)registry.GetValue("InPlace", "False") == "True";
            checkBoxBackup.Checked = (string)registry.GetValue("BackupSource", "True") == "True";
            checkBoxDemo.Checked = (string)registry.GetValue("DemoMode", "True") == "True";
            numericUpDownThreads.Value = (int)registry.GetValue("Threads", 4);
            textBoxOutput.Text = (string)registry.GetValue("OutputDir", "");
            checkBoxClear.Checked = (string)registry.GetValue("ClearOutput", "False") == "True";
            checkBoxVanilla.Checked = (string)registry.GetValue("IncludeVanilla", "True") == "True";
            checkBoxLocalization.Checked = (string)registry.GetValue("IncludeLocal", "True") == "True";
            comboBoxGame.SelectedIndex = (int)registry.GetValue("GameVersion", 0);
            comboBoxLanguage.SelectedIndex = (int)registry.GetValue("Language", 0);

            checkBoxDistribution.Checked = (string)registry.GetValue("GenerateDistribution", "False") == "True";
            textBoxTitleEng.Text = (string)registry.GetValue("EngTitle", "");
            textBoxTitleRus.Text = (string)registry.GetValue("RusTitle", "");
            textBoxTitleShort.Text = (string)registry.GetValue("ShortTitle", "");
            textBoxVersion.Text = (string)registry.GetValue("Version", "1.0");
            textBoxIntention.Text = (string)registry.GetValue("IntendedUse", "");
            textBoxSite.Text = (string)registry.GetValue("ModSite", "");
            textBoxDownloads.Text = (string)registry.GetValue("DownloadLinks", "");

            checkBoxScripts.Checked = (string)registry.GetValue("DoScripts", "True") == "True";
            checkBoxLtx.Checked = (string)registry.GetValue("DoConfigs", "True") == "True";
            checkBoxXml.Checked = (string)registry.GetValue("DoXml", "True") == "True";
            checkBoxStrings.Checked = (string)registry.GetValue("DoStrings", "True") == "True";
            checkBoxOther.Checked = (string)registry.GetValue("DoOther", "True") == "True";
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            registry.SetValue("InputDir", textBoxInput.Text);
            registry.SetValue("InPlace", checkBoxInPlace.Checked);
            registry.SetValue("BackupSource", checkBoxBackup.Checked);
            registry.SetValue("DemoMode", checkBoxDemo.Checked);
            registry.SetValue("Threads", (int)numericUpDownThreads.Value);
            registry.SetValue("OutputDir", textBoxOutput.Text);
            registry.SetValue("ClearOutput", checkBoxClear.Checked);
            registry.SetValue("IncludeVanilla", checkBoxVanilla.Checked);
            registry.SetValue("IncludeLocal", checkBoxLocalization.Checked);
            registry.SetValue("GameVersion", comboBoxGame.SelectedIndex);
            registry.SetValue("Language", comboBoxLanguage.SelectedIndex);

            registry.SetValue("GenerateDistribution", checkBoxDistribution.Checked);
            registry.SetValue("EngTitle", textBoxTitleEng.Text);
            registry.SetValue("RusTitle", textBoxTitleRus.Text);
            registry.SetValue("ShortTitle", textBoxTitleShort.Text);
            registry.SetValue("Version", textBoxVersion.Text);
            registry.SetValue("IntendedUse", textBoxIntention.Text);
            registry.SetValue("ModSite", textBoxSite.Text);
            registry.SetValue("DownloadLinks", textBoxDownloads.Text);

            registry.SetValue("DoScripts", checkBoxScripts.Checked);
            registry.SetValue("DoConfigs", checkBoxLtx.Checked);
            registry.SetValue("DoXml", checkBoxXml.Checked);
            registry.SetValue("DoStrings", checkBoxStrings.Checked);
            registry.SetValue("DoOther", checkBoxOther.Checked);

            foreach (Control control in mainControls)
            {
                controlStatus[control] = control.Enabled;
                control.Enabled = false;
            }

            PDDOptions.InputDir = textBoxInput.Text;
            PDDOptions.OutputDir = textBoxOutput.Text;
            PDDOptions.InPlace = checkBoxInPlace.Checked;
            PDDOptions.BackupSource = checkBoxBackup.Checked;
            PDDOptions.DemoMode = checkBoxDemo.Checked;
            PDDOptions.Threads = (int)numericUpDownThreads.Value;
            PDDOptions.ClearOutput = checkBoxClear.Checked;
            PDDOptions.IncludeVanilla = checkBoxVanilla.Checked;
            PDDOptions.IncludeLocal = checkBoxLocalization.Checked;
            PDDOptions.SetGame(comboBoxGame.SelectedIndex);

            PDDOptions.GenDistribution = checkBoxDistribution.Checked;
            PDDOptions.TitleEng = textBoxTitleEng.Text;
            PDDOptions.TitleRus = textBoxTitleRus.Text;
            PDDOptions.TitleShort = textBoxTitleShort.Text;
            PDDOptions.PatchVersion = textBoxVersion.Text;
            PDDOptions.IntendedUse = textBoxIntention.Text;
            PDDOptions.ModSite = textBoxSite.Text;
            PDDOptions.DownloadLinks = textBoxDownloads.Text;

            // Remember to set this AFTER GenDistribution
            PDDOptions.SetLanguages(comboBoxLanguage.SelectedIndex);

            PDDOptions.DoScripts = checkBoxScripts.Checked;
            PDDOptions.DoXml = checkBoxXml.Checked;
            PDDOptions.DoLtx = checkBoxLtx.Checked;
            PDDOptions.DoStrings = checkBoxStrings.Checked;
            PDDOptions.DoOther = checkBoxOther.Checked;

            ProgressForm progressForm = new ProgressForm();
            Thread thread = new Thread(() => Translator.Translate(this, progressForm));
            thread.IsBackground = true;
            thread.Start();
            progressForm.ShowDialog();
        }

        public void Finish()
        {
            this.Invoke((MethodInvoker)delegate
            {
                foreach (Control control in mainControls)
                {
                    control.Enabled = controlStatus[control];
                }
            });
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxTitleEng.Clear();
            textBoxTitleRus.Clear();
            textBoxTitleShort.Clear();
            textBoxVersion.Text = "1.0";
            textBoxIntention.Clear();
            textBoxSite.Clear();
            textBoxDownloads.Clear();
        }

        private void checkBoxClear_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxClear.Checked)
                checkBoxInPlace.Checked = false;
        }

        private void checkBoxDistribution_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxLanguage.Enabled = !checkBoxDistribution.Checked;
            if (checkBoxDistribution.Checked)
                checkBoxInPlace.Checked = false;
            foreach (Control control in distributionOptions)
                control.Enabled = checkBoxDistribution.Checked;
        }

        private void checkBoxInPlace_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxBackup.Enabled = checkBoxInPlace.Checked;
            textBoxOutput.Enabled = !checkBoxInPlace.Checked;
            if (checkBoxInPlace.Checked)
            {
                checkBoxClear.Checked = false;
                checkBoxDistribution.Checked = false;
            }
        }
    }
}
