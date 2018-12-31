namespace PDD_Translate_Automatic
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxInput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.checkBoxDistribution = new System.Windows.Forms.CheckBox();
            this.groupBoxDistribution = new System.Windows.Forms.GroupBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.textBoxIntention = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBoxDownloads = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxSite = new System.Windows.Forms.TextBox();
            this.textBoxVersion = new System.Windows.Forms.TextBox();
            this.textBoxTitleRus = new System.Windows.Forms.TextBox();
            this.textBoxTitleShort = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxTitleEng = new System.Windows.Forms.TextBox();
            this.comboBoxGame = new System.Windows.Forms.ComboBox();
            this.checkBoxScripts = new System.Windows.Forms.CheckBox();
            this.checkBoxLtx = new System.Windows.Forms.CheckBox();
            this.checkBoxXml = new System.Windows.Forms.CheckBox();
            this.checkBoxStrings = new System.Windows.Forms.CheckBox();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.checkBoxClear = new System.Windows.Forms.CheckBox();
            this.checkBoxInPlace = new System.Windows.Forms.CheckBox();
            this.checkBoxVanilla = new System.Windows.Forms.CheckBox();
            this.checkBoxBackup = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.checkBoxOther = new System.Windows.Forms.CheckBox();
            this.checkBoxLocalization = new System.Windows.Forms.CheckBox();
            this.checkBoxDemo = new System.Windows.Forms.CheckBox();
            this.numericUpDownThreads = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBoxDistribution.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreads)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input Directory";
            // 
            // textBoxInput
            // 
            this.textBoxInput.Location = new System.Drawing.Point(12, 25);
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Size = new System.Drawing.Size(450, 20);
            this.textBoxInput.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Output Directory";
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(12, 87);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(450, 20);
            this.textBoxOutput.TabIndex = 1;
            // 
            // checkBoxDistribution
            // 
            this.checkBoxDistribution.AutoSize = true;
            this.checkBoxDistribution.Location = new System.Drawing.Point(10, 19);
            this.checkBoxDistribution.Name = "checkBoxDistribution";
            this.checkBoxDistribution.Size = new System.Drawing.Size(123, 17);
            this.checkBoxDistribution.TabIndex = 5;
            this.checkBoxDistribution.Text = "Generate distribution";
            this.checkBoxDistribution.UseVisualStyleBackColor = true;
            this.checkBoxDistribution.CheckedChanged += new System.EventHandler(this.checkBoxDistribution_CheckedChanged);
            // 
            // groupBoxDistribution
            // 
            this.groupBoxDistribution.Controls.Add(this.checkBoxDistribution);
            this.groupBoxDistribution.Controls.Add(this.buttonClear);
            this.groupBoxDistribution.Controls.Add(this.textBoxIntention);
            this.groupBoxDistribution.Controls.Add(this.label10);
            this.groupBoxDistribution.Controls.Add(this.textBoxDownloads);
            this.groupBoxDistribution.Controls.Add(this.label8);
            this.groupBoxDistribution.Controls.Add(this.label7);
            this.groupBoxDistribution.Controls.Add(this.textBoxSite);
            this.groupBoxDistribution.Controls.Add(this.textBoxVersion);
            this.groupBoxDistribution.Controls.Add(this.textBoxTitleRus);
            this.groupBoxDistribution.Controls.Add(this.textBoxTitleShort);
            this.groupBoxDistribution.Controls.Add(this.label6);
            this.groupBoxDistribution.Controls.Add(this.label5);
            this.groupBoxDistribution.Controls.Add(this.label4);
            this.groupBoxDistribution.Controls.Add(this.label3);
            this.groupBoxDistribution.Controls.Add(this.textBoxTitleEng);
            this.groupBoxDistribution.Location = new System.Drawing.Point(12, 216);
            this.groupBoxDistribution.Name = "groupBoxDistribution";
            this.groupBoxDistribution.Size = new System.Drawing.Size(450, 249);
            this.groupBoxDistribution.TabIndex = 6;
            this.groupBoxDistribution.TabStop = false;
            this.groupBoxDistribution.Text = "Distribution";
            // 
            // buttonClear
            // 
            this.buttonClear.Enabled = false;
            this.buttonClear.Location = new System.Drawing.Point(369, 19);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 23);
            this.buttonClear.TabIndex = 1;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // textBoxIntention
            // 
            this.textBoxIntention.Enabled = false;
            this.textBoxIntention.Location = new System.Drawing.Point(7, 140);
            this.textBoxIntention.Name = "textBoxIntention";
            this.textBoxIntention.Size = new System.Drawing.Size(207, 20);
            this.textBoxIntention.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 124);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 13);
            this.label10.TabIndex = 18;
            this.label10.Text = "Intended Use";
            // 
            // textBoxDownloads
            // 
            this.textBoxDownloads.Enabled = false;
            this.textBoxDownloads.Location = new System.Drawing.Point(7, 179);
            this.textBoxDownloads.Multiline = true;
            this.textBoxDownloads.Name = "textBoxDownloads";
            this.textBoxDownloads.Size = new System.Drawing.Size(437, 58);
            this.textBoxDownloads.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 163);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(83, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Download Links";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(227, 124);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Mod Site";
            // 
            // textBoxSite
            // 
            this.textBoxSite.Enabled = false;
            this.textBoxSite.Location = new System.Drawing.Point(230, 140);
            this.textBoxSite.Name = "textBoxSite";
            this.textBoxSite.Size = new System.Drawing.Size(214, 20);
            this.textBoxSite.TabIndex = 8;
            // 
            // textBoxVersion
            // 
            this.textBoxVersion.Enabled = false;
            this.textBoxVersion.Location = new System.Drawing.Point(230, 97);
            this.textBoxVersion.Name = "textBoxVersion";
            this.textBoxVersion.Size = new System.Drawing.Size(214, 20);
            this.textBoxVersion.TabIndex = 6;
            // 
            // textBoxTitleRus
            // 
            this.textBoxTitleRus.Enabled = false;
            this.textBoxTitleRus.Location = new System.Drawing.Point(230, 55);
            this.textBoxTitleRus.Name = "textBoxTitleRus";
            this.textBoxTitleRus.Size = new System.Drawing.Size(214, 20);
            this.textBoxTitleRus.TabIndex = 4;
            // 
            // textBoxTitleShort
            // 
            this.textBoxTitleShort.Enabled = false;
            this.textBoxTitleShort.Location = new System.Drawing.Point(7, 97);
            this.textBoxTitleShort.Name = "textBoxTitleShort";
            this.textBoxTitleShort.Size = new System.Drawing.Size(207, 20);
            this.textBoxTitleShort.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(227, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Version";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 82);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Short Title";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(227, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Title (Russian)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Title (English)";
            // 
            // textBoxTitleEng
            // 
            this.textBoxTitleEng.Enabled = false;
            this.textBoxTitleEng.Location = new System.Drawing.Point(7, 55);
            this.textBoxTitleEng.Name = "textBoxTitleEng";
            this.textBoxTitleEng.Size = new System.Drawing.Size(208, 20);
            this.textBoxTitleEng.TabIndex = 3;
            // 
            // comboBoxGame
            // 
            this.comboBoxGame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGame.FormattingEnabled = true;
            this.comboBoxGame.Items.AddRange(new object[] {
            "Shadow of Chernobyl",
            "Clear Sky",
            "Call of Pripyat",
            "Call of Chernobyl [1.4.22]"});
            this.comboBoxGame.Location = new System.Drawing.Point(12, 149);
            this.comboBoxGame.Name = "comboBoxGame";
            this.comboBoxGame.Size = new System.Drawing.Size(450, 21);
            this.comboBoxGame.TabIndex = 2;
            // 
            // checkBoxScripts
            // 
            this.checkBoxScripts.AutoSize = true;
            this.checkBoxScripts.Checked = true;
            this.checkBoxScripts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxScripts.Location = new System.Drawing.Point(12, 477);
            this.checkBoxScripts.Name = "checkBoxScripts";
            this.checkBoxScripts.Size = new System.Drawing.Size(58, 17);
            this.checkBoxScripts.TabIndex = 3;
            this.checkBoxScripts.Text = "Scripts";
            this.checkBoxScripts.UseVisualStyleBackColor = true;
            // 
            // checkBoxLtx
            // 
            this.checkBoxLtx.AutoSize = true;
            this.checkBoxLtx.Checked = true;
            this.checkBoxLtx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLtx.Location = new System.Drawing.Point(157, 477);
            this.checkBoxLtx.Name = "checkBoxLtx";
            this.checkBoxLtx.Size = new System.Drawing.Size(72, 17);
            this.checkBoxLtx.TabIndex = 4;
            this.checkBoxLtx.Text = "Loose Ltx";
            this.checkBoxLtx.UseVisualStyleBackColor = true;
            // 
            // checkBoxXml
            // 
            this.checkBoxXml.AutoSize = true;
            this.checkBoxXml.Checked = true;
            this.checkBoxXml.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxXml.Location = new System.Drawing.Point(76, 477);
            this.checkBoxXml.Name = "checkBoxXml";
            this.checkBoxXml.Size = new System.Drawing.Size(75, 17);
            this.checkBoxXml.TabIndex = 5;
            this.checkBoxXml.Text = "Loose Xml";
            this.checkBoxXml.UseVisualStyleBackColor = true;
            // 
            // checkBoxStrings
            // 
            this.checkBoxStrings.AutoSize = true;
            this.checkBoxStrings.Checked = true;
            this.checkBoxStrings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxStrings.Location = new System.Drawing.Point(235, 477);
            this.checkBoxStrings.Name = "checkBoxStrings";
            this.checkBoxStrings.Size = new System.Drawing.Size(88, 17);
            this.checkBoxStrings.TabIndex = 6;
            this.checkBoxStrings.Text = "String Tables";
            this.checkBoxStrings.UseVisualStyleBackColor = true;
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Location = new System.Drawing.Point(387, 471);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(75, 23);
            this.buttonTranslate.TabIndex = 7;
            this.buttonTranslate.Text = "Translate";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // checkBoxClear
            // 
            this.checkBoxClear.AutoSize = true;
            this.checkBoxClear.Location = new System.Drawing.Point(12, 113);
            this.checkBoxClear.Name = "checkBoxClear";
            this.checkBoxClear.Size = new System.Drawing.Size(126, 17);
            this.checkBoxClear.TabIndex = 16;
            this.checkBoxClear.Text = "Clear output directory";
            this.checkBoxClear.UseVisualStyleBackColor = true;
            this.checkBoxClear.CheckedChanged += new System.EventHandler(this.checkBoxClear_CheckedChanged);
            // 
            // checkBoxInPlace
            // 
            this.checkBoxInPlace.AutoSize = true;
            this.checkBoxInPlace.Location = new System.Drawing.Point(12, 51);
            this.checkBoxInPlace.Name = "checkBoxInPlace";
            this.checkBoxInPlace.Size = new System.Drawing.Size(110, 17);
            this.checkBoxInPlace.TabIndex = 17;
            this.checkBoxInPlace.Text = "Translate in-place";
            this.checkBoxInPlace.UseVisualStyleBackColor = true;
            this.checkBoxInPlace.CheckedChanged += new System.EventHandler(this.checkBoxInPlace_CheckedChanged);
            // 
            // checkBoxVanilla
            // 
            this.checkBoxVanilla.AutoSize = true;
            this.checkBoxVanilla.Location = new System.Drawing.Point(144, 113);
            this.checkBoxVanilla.Name = "checkBoxVanilla";
            this.checkBoxVanilla.Size = new System.Drawing.Size(114, 17);
            this.checkBoxVanilla.TabIndex = 18;
            this.checkBoxVanilla.Text = "Include vanilla text";
            this.checkBoxVanilla.UseVisualStyleBackColor = true;
            // 
            // checkBoxBackup
            // 
            this.checkBoxBackup.AutoSize = true;
            this.checkBoxBackup.Enabled = false;
            this.checkBoxBackup.Location = new System.Drawing.Point(144, 51);
            this.checkBoxBackup.Name = "checkBoxBackup";
            this.checkBoxBackup.Size = new System.Drawing.Size(122, 17);
            this.checkBoxBackup.TabIndex = 19;
            this.checkBoxBackup.Text = "Back up source files";
            this.checkBoxBackup.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 133);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(35, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "Game";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(11, 173);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 13);
            this.label11.TabIndex = 21;
            this.label11.Text = "Target language";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Items.AddRange(new object[] {
            "English",
            "French",
            "German",
            "Italian",
            "Spanish"});
            this.comboBoxLanguage.Location = new System.Drawing.Point(12, 189);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(450, 21);
            this.comboBoxLanguage.TabIndex = 22;
            // 
            // checkBoxOther
            // 
            this.checkBoxOther.AutoSize = true;
            this.checkBoxOther.Checked = true;
            this.checkBoxOther.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOther.Location = new System.Drawing.Point(329, 477);
            this.checkBoxOther.Name = "checkBoxOther";
            this.checkBoxOther.Size = new System.Drawing.Size(52, 17);
            this.checkBoxOther.TabIndex = 23;
            this.checkBoxOther.Text = "Other";
            this.checkBoxOther.UseVisualStyleBackColor = true;
            // 
            // checkBoxLocalization
            // 
            this.checkBoxLocalization.AutoSize = true;
            this.checkBoxLocalization.Location = new System.Drawing.Point(272, 113);
            this.checkBoxLocalization.Name = "checkBoxLocalization";
            this.checkBoxLocalization.Size = new System.Drawing.Size(116, 17);
            this.checkBoxLocalization.TabIndex = 25;
            this.checkBoxLocalization.Text = "Include localization";
            this.checkBoxLocalization.UseVisualStyleBackColor = true;
            // 
            // checkBoxDemo
            // 
            this.checkBoxDemo.AutoSize = true;
            this.checkBoxDemo.Location = new System.Drawing.Point(272, 51);
            this.checkBoxDemo.Name = "checkBoxDemo";
            this.checkBoxDemo.Size = new System.Drawing.Size(83, 17);
            this.checkBoxDemo.TabIndex = 26;
            this.checkBoxDemo.Text = "Demo mode";
            this.checkBoxDemo.UseVisualStyleBackColor = true;
            // 
            // numericUpDownThreads
            // 
            this.numericUpDownThreads.Location = new System.Drawing.Point(381, 48);
            this.numericUpDownThreads.Name = "numericUpDownThreads";
            this.numericUpDownThreads.Size = new System.Drawing.Size(35, 20);
            this.numericUpDownThreads.TabIndex = 27;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(416, 52);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(46, 13);
            this.label12.TabIndex = 28;
            this.label12.Text = "Threads";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 506);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.numericUpDownThreads);
            this.Controls.Add(this.checkBoxDemo);
            this.Controls.Add(this.checkBoxLocalization);
            this.Controls.Add(this.checkBoxOther);
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.checkBoxBackup);
            this.Controls.Add(this.comboBoxGame);
            this.Controls.Add(this.checkBoxVanilla);
            this.Controls.Add(this.checkBoxInPlace);
            this.Controls.Add(this.checkBoxClear);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.checkBoxStrings);
            this.Controls.Add(this.checkBoxXml);
            this.Controls.Add(this.checkBoxLtx);
            this.Controls.Add(this.checkBoxScripts);
            this.Controls.Add(this.groupBoxDistribution);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxInput);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "PDD Translate Automatic";
            this.groupBoxDistribution.ResumeLayout(false);
            this.groupBoxDistribution.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreads)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxInput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.CheckBox checkBoxDistribution;
        private System.Windows.Forms.GroupBox groupBoxDistribution;
        private System.Windows.Forms.TextBox textBoxDownloads;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxSite;
        private System.Windows.Forms.TextBox textBoxVersion;
        private System.Windows.Forms.TextBox textBoxTitleRus;
        private System.Windows.Forms.TextBox textBoxTitleShort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxTitleEng;
        private System.Windows.Forms.CheckBox checkBoxScripts;
        private System.Windows.Forms.CheckBox checkBoxLtx;
        private System.Windows.Forms.CheckBox checkBoxXml;
        private System.Windows.Forms.CheckBox checkBoxStrings;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.ComboBox comboBoxGame;
        private System.Windows.Forms.CheckBox checkBoxClear;
        private System.Windows.Forms.TextBox textBoxIntention;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.CheckBox checkBoxInPlace;
        private System.Windows.Forms.CheckBox checkBoxVanilla;
        private System.Windows.Forms.CheckBox checkBoxBackup;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.CheckBox checkBoxOther;
        private System.Windows.Forms.CheckBox checkBoxLocalization;
        private System.Windows.Forms.CheckBox checkBoxDemo;
        private System.Windows.Forms.NumericUpDown numericUpDownThreads;
        private System.Windows.Forms.Label label12;
    }
}

