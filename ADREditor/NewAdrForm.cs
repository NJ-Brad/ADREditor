using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;

namespace ADREditor
{
    public partial class NewAdrForm : Form
    {
        public NewAdrForm()
        {
            InitializeComponent();
        }

        public string Title { get; set; }
        public string Replaces { get; set; }
        public string AdrFolder { get; set; }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            comboBox1.Items.Add("-None-");
            foreach (string str in Directory.GetFiles(AdrFolder, "*.adr"))
            {
                comboBox1.Items.Add(Path.GetFileNameWithoutExtension(str));
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Title = textBox1.Text;
            if (comboBox1.SelectedIndex > 0)
            {
                Replaces = comboBox1.SelectedItem.ToString();
            }
            else
            {
                Replaces = string.Empty;
            }
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
