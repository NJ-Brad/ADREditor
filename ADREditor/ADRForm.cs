using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ADREditor
{
    public partial class ADRForm : Form
    {
        //        string workingFolder = @"C:\Autoany\ADR";
        string workingFolder = "";

        public ADRForm()
        {
            InitializeComponent();
        }

        private void ListDecisions()
        {
            Directory.GetFiles(workingFolder, "*.adr");
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LoadDecisons();
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
        }

        private void LoadDecisons()
        {
            listBox1.Items.Clear();
            if (!string.IsNullOrEmpty(workingFolder))
            {
                foreach (string str in Directory.GetFiles(workingFolder, "*.adr"))
                {
                    listBox1.Items.Add(Path.GetFileNameWithoutExtension(str));
                }

                Text = $"ADR Editor - {workingFolder}";
            }
        }


        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string fileName = "";

            NewAdrForm naf = new NewAdrForm();
            naf.AdrFolder = workingFolder;
            if (naf.ShowDialog() == DialogResult.OK)
            {
                fileName = WriteNew(naf.Title);
                LoadDecisons();

                if (listBox1.Items.Count > 0)
                {
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                }
            }
        }

        private string WriteNew(string title)
        {
            var fileNumber = Directory.Exists(this.workingFolder)
                ? GetNextFileNumber(this.workingFolder)
                : 1;
            string fileName = Path.Combine(
                workingFolder,
                $"{fileNumber.ToString().PadLeft(4, '0')}-{SanitizeFileName(title)}.adr");

            using (var writer = File.CreateText(fileName))
            {
                writer.WriteLine($"# {fileNumber}. {title}");
                writer.WriteLine();
                writer.WriteLine(DateTime.Today.ToString("yyyy-MM-dd"));
                writer.WriteLine();
                writer.WriteLine("## Status");
                writer.WriteLine();
                writer.WriteLine("Proposed");
                writer.WriteLine();
                writer.WriteLine("## Context");
                writer.WriteLine();
                writer.WriteLine("{context}");
                writer.WriteLine();
                writer.WriteLine("## Decision");
                writer.WriteLine();
                writer.WriteLine("{decision}");
                writer.WriteLine();
                writer.WriteLine("## Consequences");
                writer.WriteLine();
                writer.WriteLine("{consequences}");
            }

            return fileName;
        }

        private int GetNextFileNumber(string docFolder)
        {
            int fileNumOut = 0;
            var files =
                from file in new DirectoryInfo(workingFolder).GetFiles("*.adr", SearchOption.TopDirectoryOnly)
                let fileNum = file.Name.Substring(0, 4)
                where int.TryParse(fileNum, out fileNumOut)
                select fileNumOut;
            var maxFileNum = files.Any() ? files.Max() : 0;
            return maxFileNum + 1;
        }

        private static string SanitizeFileName(string title)
        {
            return title
                .Replace(' ', '-')
                .ToLower();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                string fileName = Path.ChangeExtension(Path.Combine(workingFolder, listBox1.SelectedItem.ToString()), ".adr");
                markDownDisplay1.FileName = fileName;
                editorControl2.LoadFileToEdit(fileName);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(workingFolder))
            {
                string fileName = Path.ChangeExtension(Path.Combine(workingFolder, listBox1.SelectedItem.ToString()), ".adr");
                markDownDisplay1.FileName = fileName;
                editorControl2.LoadFileToEdit(fileName);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions");
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "https://upmo.com/dev/decisions/");
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // file - new
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Create a folder that will hold your decisions.";
            fbd.ShowNewFolderButton = true;

            bool exit = false;
            bool ready = false;

            while (!exit)
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    if ((Directory.GetFiles(fbd.SelectedPath).Length > 0) || (Directory.GetDirectories(fbd.SelectedPath).Length > 0))
                    {
                        MessageBox.Show("You must create a new directory, or select an empty directory");
                    }
                    else
                    {
                        exit = true;
                        ready = true;
                    }
                }
                else
                {
                    exit = true;
                }
            }

            if (ready)
            {
                workingFolder = fbd.SelectedPath;
                // I need to clear the view / edit controls, or create default ADR records
                markDownDisplay1.Clear();
                editorControl2.Clear();
                LoadDecisons();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // file - open
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the folder that holds your decisions.";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                workingFolder = fbd.SelectedPath;
                LoadDecisons();
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab1 = new AboutBox1();
            ab1.ShowDialog();
        }
    }
}
