using System;
using System.Linq;
using System.Windows.Forms;

namespace MarkDownHelper
{
    public partial class LinkForm : Form
    {
        public LinkForm()
        {
            InitializeComponent();
        }

        public string Display {
            get {
                return this.display;
            }
            set {
                this.display = value;
            }
        }

        public string LinkText {
            get {
                return this.link;
            }
            set {
                this.link = value;
            }
        }

        public string Tooltip {
            get {
                return this.tooltip;
            }
            set {
                this.tooltip = value;
            }
        }

        public static string CreateLinkText()
        {
            string rtnVal = string.Empty;

            LinkForm form = new LinkForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                string display = string.Empty;
                string linkText = string.Empty;
                string tooltip = string.Empty;

                if (!string.IsNullOrEmpty(form.Display))
                {
                    display = form.Display;
                }
                else
                {
                    if (!string.IsNullOrEmpty(form.LinkText))
                    {
                        display = form.LinkText;
                    }
                }

                linkText = form.LinkText;
                tooltip = form.Tooltip;

                if (string.IsNullOrEmpty(linkText))
                {
                    return string.Empty;
                }

                if (!string.IsNullOrEmpty(tooltip))
                {
                    linkText = string.Format("{0} \"{1}\"", linkText, tooltip);
                }

                rtnVal = string.Format("[{0}]({1})", display, linkText);
            }
            return rtnVal;
        }

        string display = string.Empty;
        string link = string.Empty;
        string tooltip = string.Empty;

        private void button1_Click(object sender, EventArgs e)
        {
            display = textBox1.Text;
            link = textBox2.Text;
            tooltip = textBox3.Text;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
