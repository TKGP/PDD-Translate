using System;
using System.Windows.Forms;


namespace PDDTranslate
{
    using TranslateOption = Translator.TranslateOption;

    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            Translator.SetTranslateOptions(
                radioButton6.Checked ? TranslateOption.Auto : (radioButton5.Checked ? TranslateOption.Semi : (radioButton4.Checked ? TranslateOption.Manual : TranslateOption.Skip)),
                radioButton1.Checked ? TranslateOption.Auto : (radioButton2.Checked ? TranslateOption.Semi : (radioButton3.Checked ? TranslateOption.Manual : TranslateOption.Skip)),
                radioButton9.Checked ? TranslateOption.Auto : (radioButton8.Checked ? TranslateOption.Semi : (radioButton7.Checked ? TranslateOption.Manual : TranslateOption.Skip)),
                radioButton12.Checked ? TranslateOption.Auto : (radioButton11.Checked ? TranslateOption.Semi : (radioButton10.Checked ? TranslateOption.Manual : TranslateOption.Skip)),
                checkBox1.Checked
                );

            this.DialogResult = DialogResult.OK;
        }
    }
}
