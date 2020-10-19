using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarkDownHelper
{
    public partial class TableForm : Form
    {
        public TableForm()
        {
            InitializeComponent();
        }
        public string TableText {
            get {
                return this.tableText;
            }
            set {
                this.tableText = value;
            }
        }

        public static string CreateTableText()
        {
            string rtnVal = string.Empty;

            TableForm form = new TableForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                rtnVal = form.TableText;
                //rtnVal = string.Format("[{0}]({1})", display, linkText);
            }
            return rtnVal;
        }

        string tableText = string.Empty;

        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder sbTitles = new StringBuilder("| ");
            StringBuilder sbLines = new StringBuilder("| ");

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    sbTitles.Append(row.Cells[0].Value.ToString());
                    sbTitles.Append(" | ");

                    switch (row.Cells[1].Value.ToString())
                    {
                        case "Left":
                            sbLines.Append(new string('-', InsureMinimumMet(row.Cells[0].Value.ToString().Length, 3)));
                            break;
                        case "Center":
                            sbLines.Append(":" + new string('-', InsureMinimumMet(row.Cells[0].Value.ToString().Length - 2, 3)) + ":");
                            break;
                        case "Right":
                            sbLines.Append(new string('-', InsureMinimumMet(row.Cells[0].Value.ToString().Length - 1, 3)) + ":");
                            break;
                    }
                    sbLines.Append(" | ");
                }
            }

            tableText = sbTitles.ToString() + "\n" + sbLines.ToString() + "\n";

            DialogResult = DialogResult.OK;
            Close();
        }

        private int InsureMinimumMet(int value, int minValue) { 
            int rtnVal = value;
            if (value < minValue)
            {
                rtnVal = minValue;
            }
            return rtnVal;
        }
    }
}
