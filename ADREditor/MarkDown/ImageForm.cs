using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MarkDownHelper
{
    public partial class ImageForm : Form
    {
        string baseDir = string.Empty;

        public ImageForm(string baseDir)
        {
            InitializeComponent();
            this.baseDir = baseDir;

            // http://stackoverflow.com/questions/2069048/setting-the-filter-to-an-openfiledialog-to-allow-the-typical-image-formats
            //var imageExtensions = string.Join(";", ImageCodecInfo.GetImageDecoders().Select(ici => ici.FilenameExtension)); dialog.Filter = string.Format("Images|{0}|All Files|*.*", imageExtensions);

            string[] allowedExtensions = new string[]{"BMP","DIB","RLE","JPG","JPEG","JPE","JFIF","GIF","TIF","TIFF","PNG"};

            foreach (string fileName in Directory.GetFiles(baseDir))
            {
                string check = fileName.ToUpper();
                string[] parts = check.Split('.');
                string lastPart = parts[parts.Length - 1];
                if (allowedExtensions.Contains<string>(lastPart))
                {
                    listBox1.Items.Add(Path.GetFileName(fileName));
                }
            }
        }

        public string Display
        {
            get
            {
                return this.display;
            }
            set
            {
                this.display = value;
            }
        }

        public string LinkText
        {
            get
            {
                return this.link;
            }
            set
            {
                this.link = value;
            }
        }

        public string Tooltip
        {
            get
            {
                return this.tooltip;
            }
            set
            {
                this.tooltip = value;
            }
        }

        public static string CreateImageText(string baseDirectory)
        {
            string rtnVal = string.Empty;

            ImageForm form = new ImageForm(baseDirectory);
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

                rtnVal = string.Format("![{0}]({1})", display, linkText);
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

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            textBox2.Text = string.Format("./{0}", listBox1.SelectedItem);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Images|*.BMP;*.DIB;*.RLE;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG|BMP Files: (*.BMP;*.DIB;*.RLE)|*.BMP;*.DIB;*.RLE|JPEG Files: (*.JPG;*.JPEG;*.JPE;*.JFIF)|*.JPG;*.JPEG;*.JPE;*.JFIF|GIF Files: (*.GIF)|*.GIF|TIFF Files: (*.TIF;*.TIFF)|*.TIF;*.TIFF|PNG Files: (*.PNG)|*.PNG|All Files (*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string str in ofd.FileNames)
                {
                    if (Path.GetDirectoryName(str) != baseDir)
                    {
                        string newPath = Path.Combine(baseDir, Path.GetFileName(str));
                        if (File.Exists(newPath))
                        {
                            if (MessageBox.Show("Overwrite file?", "Warning", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                File.Copy(str, newPath, true);
                                textBox2.Text = string.Format("./{0}", listBox1.SelectedItem);
                            }
                        }
                        else
                        {
                            File.Copy(str, newPath, true);
                            listBox1.Items.Add(Path.GetFileName(newPath));
                            textBox2.Text = string.Format("./{0}", Path.GetFileName(newPath));
                        }
                    }
                }
            }

        }
    }
}
