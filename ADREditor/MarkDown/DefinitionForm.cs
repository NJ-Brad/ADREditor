using System;
using System.Linq;
using System.Windows.Forms;

namespace MarkDownHelper
{
    public partial class DefinitionForm : Form
    {
        public DefinitionForm()
        {
            InitializeComponent();
        }

        public string Term {
            get {
                return this.term;
            }
            set {
                this.term = value;
            }
        }

        public string Meaning {
            get {
                return this.meaning;
            }
            set {
                this.meaning = value;
            }
        }

        public static string CreateDefinitionText()
        {
            string rtnVal = string.Empty;

            DefinitionForm form = new DefinitionForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                rtnVal = string.Format("<dt>{0}</dt>\n<dd>{1}</dd>\n", form.Term, form.Meaning);
            }
            return rtnVal;
        }

        string term = string.Empty;
        string meaning = string.Empty;

        private void button1_Click(object sender, EventArgs e)
        {
            term = textBox1.Text;
            meaning = textBox2.Text;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
