using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MarkDownHelper
{
    public delegate string OperationDelegate(string s);

    public partial class EditorControl : UserControl
    {
        public EditorControl()
        {
            InitializeComponent();
            // http://stackoverflow.com/questions/4823468/comments-in-markdown
            // (empty line)
            // [comment]: # (This actually is the most platform independent comment)

            // https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet
            // I didn't implement the Youtube video option
            operations.Add("H1", "# ");
            operations.Add("H2", "## ");
            operations.Add("H3", "### ");
            operations.Add("H4", "#### ");
            operations.Add("H5", "##### ");
            operations.Add("H6", "###### ");
            operations.Add("Alt-H1", string.Empty, t => { return "\r\n" + new string('=', t.Length); });
            operations.Add("Alt-H2", string.Empty, t => { return "\r\n" + new string('-', t.Length); });
            operations.Add("Italic", "_", "_");
            operations.Add("Bold", "**", "**");
            operations.Add("Strike", "~~", "~~");
            operations.Add("HR", "\r\n----");
            operations.Add("Link", t => { return LinkForm.CreateLinkText(); }, string.Empty);
            operations.Add("Ordered List", "1. ");
            operations.Add("Unordered List", "+ ");
            operations.Add("Image", t => { return ImageForm.CreateImageText(Path.GetDirectoryName(fileName)); }, string.Empty);
            operations.Add("Code", t => { return CodeForm.CreateLanguageText(); }, "\n```");
            operations.Add("Quote", t => { StringBuilder sb = new StringBuilder("\n" + t); sb.Replace("\n", "\n> "); return sb.ToString().TrimStart().TrimEnd(new char[] { '\n', '>', ' ' }); }, string.Empty);
            operations["Quote"].KeepOriginal = false;
            operations.Add("Dictionary", "<dl>\n", "</dl>");
            operations.Add("Definition", t => { return DefinitionForm.CreateDefinitionText(); }, string.Empty);
            operations.Add("Header", t => { return TableForm.CreateTableText(); }, string.Empty);
            operations.Add("Row", string.Empty, t =>
            {
                int cols = t.Occurrences('|') + 1; if (t.StartsWith("|")) cols--; if (t.TrimEnd().EndsWith("|")) cols--;
                StringBuilder sb = new StringBuilder("\n|"); for (int i = 0; i < cols; i++) { sb.Append("value|"); }
                return sb.ToString();
            });
            operations.Add("Task", "- [ ]", string.Empty);

            autoSaveTimer = new Timer();
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            autoSaveTimer.Interval = 3000;
            autoSaveTimer.Enabled = false;
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            autoSaveTimer.Stop();
            autoSaveTimer.Enabled = false;

            File.WriteAllText(fileName, richTextBox1.Text);
            dirty = false;
            Show();
        }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                this.fileName = value;
            }
        }

        public static void Edit(string fileName)
        {
            //Editor ed = new Editor();
            //ed.FileName = fileName;
            //ed.ShowDialog();
        }

        //protected override void OnShown(EventArgs e)
        //{
        //    base.OnShown(e);
        //    if (!string.IsNullOrEmpty(fileName))
        //    {
        //        openToolStripMenuItem.Enabled = false;
        //        saveAsToolStripMenuItem.Enabled = false;
        //        LoadFileToEdit(fileName);
        //    }
        //}

        Operations operations = new Operations();

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Show();
        }

        int hPos = 0;
        int vPos = 0;

        private new void Show()
        {
            base.Show();

            // https://stackoverflow.com/questions/1277691/how-to-retrieve-the-scrollbar-position-of-the-webbrowser-control-in-net
            System.Windows.Forms.HtmlDocument htmlDoc = webBrowser1.Document;
            if (htmlDoc != null)
            {
                vPos = htmlDoc.GetElementsByTagName("HTML")[0].ScrollTop;
                hPos = htmlDoc.GetElementsByTagName("HTML")[0].ScrollLeft;
            }

            ShowText(richTextBox1.Text, webBrowser1);
        }

        private void ShowText(string rawText, WebBrowser control)
        {
            string readmeText = rawText;

            // https://talk.commonmark.org/t/markdig-markdown-processor-for-net/2106

            //https://github.com/lunet-io/markdig
            // https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor
            // http://markdownpad.com/
            // https://guides.github.com/pdfs/markdown-cheatsheet-online.pdf
            // https://confluence.atlassian.com/bitbucketserver/markdown-syntax-guide-776639995.html
            // https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet#tables

            // https://github.github.com/github-flavored-markdown/sample_content.html

            // Configure the pipeline with all advanced extensions active
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();

            //string val = Markdig.Markdown.ToHtml(readmeText.FixLineFeeds(), pipeline);
            //            ShowContent(control, Markdig.Markdown.ToHtml(readmeText.FixLineFeeds().TranslatePaths(Path.GetDirectoryName(fileName).GenerateToc()), pipeline));
            ShowContent(control, Markdig.Markdown.ToHtml(readmeText.TranslatePaths(Path.GetDirectoryName(fileName)).GenerateToc(), pipeline));
        }

        private void Show(string fileName, WebBrowser control)
        {
            if (File.Exists(fileName))
            {
                string readmeText = File.ReadAllText(fileName);

                // Configure the pipeline with all advanced extensions active
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();

                string val = Markdig.Markdown.ToHtml(readmeText, pipeline);
                ShowContent(control, Markdig.Markdown.ToHtml(readmeText.GenerateToc(), pipeline));
            }
            else
            {
                ShowContent(control, string.Format("<H3>No {0} found</H3>", Path.GetFileName(fileName)));
            }
        }

        private void ShowContent(WebBrowser control, string html)
        {
            // https://weblogs.asp.net/gunnarpeipman/displaying-custom-html-in-webbrowser-control
            control.Navigate("about:blank");
            if (control.Document != null)
            {
                control.Document.Write(string.Empty);
            }
            control.DocumentText = html.EnableNewerFeatures().AddGitHubStyle().TranslatePaths(Path.GetDirectoryName(fileName));
        }

        private void Modify(RichTextBox control, string prefix)
        {
            Modify(control, prefix, string.Empty, true);
        }

        private void Modify(RichTextBox control, Operation op)
        {
            string currentText = control.SelectedText.TrimEnd();

            string prefix = op.Prefix;
            string suffix = op.Suffix;

            if (op.PrefixDelegate != null)
            {
                prefix = op.PrefixDelegate(currentText);
            }

            if (op.SuffixDelegate != null)
            {
                suffix = op.SuffixDelegate(currentText);
            }

            Modify(richTextBox1, prefix, suffix, op.KeepOriginal);
        }

        private void Modify(RichTextBox control, string prefix, string suffix, bool keepOriginal)
        {
            string currentText = control.SelectedText.TrimEnd();
            bool replaceEOL = !control.SelectedText.Equals(control.SelectedText.TrimEnd());
            string newText = string.Format("{0}{1}{2}{3}", prefix, keepOriginal ? currentText : string.Empty, suffix, replaceEOL ? "\r\n" : string.Empty);
            control.SelectedText = newText;
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            string buttonText = GetButtonText(sender);

            if (operations.ContainsKey(buttonText))
            {
                Operation op = operations[buttonText];

                Modify(richTextBox1, op);
                Show();
            }
        }

        private string GetButtonText(object sender)
        {
            string rtnval = string.Empty;

            if (sender is ToolStripDropDownItem)
            {
                rtnval = (sender as ToolStripDropDownItem).Text;
            }
            if (sender is ToolStripButton)
            {
                rtnval = (sender as ToolStripButton).Text;
            }

            return rtnval;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            string text = LinkForm.CreateLinkText();
        }

        string fileName = string.Empty;

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Markdown Files (*.md)|*.md|All Files (*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadFileToEdit(ofd.FileName);
            }
        }

        public void LoadFileToEdit(string fileName)
        {
            this.fileName = fileName;
            richTextBox1.Text = File.ReadAllText(fileName);
            Show();
            dirty = false;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                File.WriteAllText(fileName, richTextBox1.Text);
            }
            dirty = false;
        }

        private void sampleTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = @"# Cake

[![NuGet](https://img.shields.io/nuget/v/Cake.svg)](https://www.nuget.org/packages/Cake) [![MyGet](https://img.shields.io/myget/cake/vpre/Cake.svg?label=myget)](https://www.myget.org/gallery/cake) [![Chocolatey](https://img.shields.io/chocolatey/v/Cake.portable.svg)](https://chocolatey.org/packages/cake.portable)
[![homebrew](https://img.shields.io/homebrew/v/cake.svg)](http://braumeister.org/formula/cake)

[![Source Browser](https://img.shields.io/badge/Browse-Source-green.svg)](http://sourcebrowser.io/Browse/cake-build/cake)

Cake (C# Make) is a build automation system with a C# DSL to do things like compiling code, copy files/folders, running unit tests, compress files and build NuGet packages.

## Continuous integration

| Build server                | Platform     | Build status                                                                                                                                                        | Integration tests                                                                                                                                                   |
|-----------------------------|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| AppVeyor                    | Windows      | [![AppVeyor branch](https://img.shields.io/appveyor/ci/cakebuild/cake/develop.svg)](https://ci.appveyor.com/project/cakebuild/cake/branch/develop)                  | [![AppVeyor branch](https://img.shields.io/appveyor/ci/cakebuild/cake-eijwj/develop.svg)](https://ci.appveyor.com/project/cakebuild/cake-eijwj)  |
| Travis                      | Linux / OS X | [![Travis build status](https://travis-ci.org/cake-build/cake.svg?branch=develop)](https://travis-ci.org/cake-build/cake)                                           |                                                                                                                                                                     |
| TeamCity                    | Windows      | [![TeamCity Build Status](http://img.shields.io/teamcity/codebetter/Cake_CakeMaster.svg)](http://teamcity.codebetter.com/viewType.html?buildTypeId=Cake_CakeMaster) |                                                                                                                                                                     |
| Bitrise                     | OS X         | ![Bitrise Build Status](https://www.bitrise.io/app/7a9d707b00881436.svg?token=m8zsF3tNONLaF03eHU-Ftg&branch=develop)                                                | ![Build Status](https://www.bitrise.io/app/804b431c1f27e0a0.svg?token=qKosHEaJAJEqzZcq4s5WRg&branch=develop)                                                        |
| Bitrise                     | Linux        | ![Bitrise Build Status](https://www.bitrise.io/app/b811c91a26b1ea80.svg?token=zdwab0niOTRF4p3HcFYaxQ&branch=develop)                                                | ![Build Status](https://www.bitrise.io/app/5a406f34f22113c6.svg?token=TQPbsmA9yP-iJOhzunIP4w&branch=develop)                                                        |
| Jenkins                     | Windows      | [![Jenkins](https://img.shields.io/jenkins/s/https/cake-jenkins.azurewebsites.net/Cake.svg)](http://cake-jenkins.azurewebsites.net/job/Cake/lastStableBuild/)       |                                                                                                                                                                     |
| Bamboo                      | Windows      | [![Bamboo Build Status](https://bambooshield.azurewebsites.net/planstatus/Flat/CAKE-CAKE.svg)](https://cakebuild.atlassian.net/builds/browse/CAKE-CAKE)             |                                                                                                                                                                     |
| Visual Studio Team Services | Windows      | ![VSTS Build Status](https://img.shields.io/vso/build/cake-build/af63183c-ac1f-4dbb-93bc-4fa862ea5809/1.svg)                                                        |                                                                                                                                                                     |
| MyGet Build Services        | Windows      | [![MyGet Build Status](https://www.myget.org/BuildSource/Badge/cake-myget-build-service?identifier=53513546-050e-45de-9500-f161c99df6e2)](https://www.myget.org/)   |  &nbsp;                                                                                                                                                             |
| Bitbucket Pipelines         | Linux        | [![Build Status](https://bitbucketshield.azurewebsites.net/status/cakebuild/cake-integration-tests/develop)](https://bitbucketshield.azurewebsites.net/url/cakebuild/cake-integration-tests/develop) | [![Build Status](https://bitbucketshield.azurewebsites.net/status/cakebuild/cake-integration-tests/IntegrationTests_develop)](https://bitbucketshield.azurewebsites.net/url/cakebuild/cake-integration-tests/IntegrationTests_develop) |

## Code Coverage

[![Coverage Status](https://coveralls.io/repos/github/cake-build/cake/badge.svg?branch=develop)](https://coveralls.io/github/cake-build/cake?branch=develop)

## Table of Contents

1. [Documentation](https://github.com/cake-build/cake#documentation)
2. [Example](https://github.com/cake-build/cake#example)
    - [Install the Cake bootstrapper](https://github.com/cake-build/cake#1-install-the-cake-bootstrapper)
    - [Create a Cake script](https://github.com/cake-build/cake#2-create-a-cake-script)
    - [Run it!](https://github.com/cake-build/cake#3-run-it)
3. [Contributing](https://github.com/cake-build/cake#contributing)
4. [Get in touch](https://github.com/cake-build/cake#get-in-touch)
5. [License](https://github.com/cake-build/cake#license)

## Documentation

You can read the latest documentation at [http://cakebuild.net/](http://cakebuild.net/).

## Example

This example downloads the Cake bootstrapper and executes a simple build script.
The bootstrapper is used to bootstrap Cake in a simple way and is not in
required in any way to execute build scripts. If you prefer to invoke the Cake
executable yourself, [take a look at the command line usage](http://cakebuild.net/docs/cli/usage).

This example is also available on our homepage:
[http://cakebuild.net/docs/tutorials/setting-up-a-new-project](http://cakebuild.net/docs/tutorials/setting-up-a-new-project)

### 1. Install the Cake bootstrapper

The bootstrapper is used to download Cake and the tools required by the
build script.

##### Windows

```powershell
Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
```

##### Linux

```console
curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/linux
```

##### OS X

```console
curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/osx
```

### 2. Create a Cake script

Add a cake script called `build.cake` to the same location as the
bootstrapper script that you downloaded.

```csharp
var target = Argument(""target"", ""Default"");

Task(""Default"")
  .Does(() =>
{
  Information(""Hello World!"");
});

RunTarget(target);
```

### 3. Run it!

##### Windows

```powershell
# Execute the bootstrapper script.
./build.ps1
```

##### Linux / OS X

```console
# Adjust the permissions for the bootstrapper script.
chmod +x build.sh

# Execute the bootstrapper script.
./build.sh
```

## Contributing

So you’re thinking about contributing to Cake? Great! It’s **really** appreciated.

Make sure you've read the [contribution guidelines](http://cakebuild.net/contribute/contribution-guidelines/) before sending that epic pull request. You'll also need to sign the [contribution license agreement](https://cla2.dotnetfoundation.org/) (CLA) for anything other than a trivial change.  **NOTE:** The .Net Foundation CLA Bot will provide a link to this CLA within the PR that you submit if it is deemed as required.

* Fork the repository.
* Create a branch to work in.
* Make your feature addition or bug fix.
* Don't forget the unit tests.
* Send a pull request.

## Get in touch

[![Follow @cakebuildnet](https://img.shields.io/badge/Twitter-Follow%20%40cakebuildnet-blue.svg)](https://twitter.com/intent/follow?screen_name=cakebuildnet)

[![Join the chat at https://gitter.im/cake-build/cake](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/cake-build/cake?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## License

Copyright © Patrik Svensson, Mattias Karlsson, Gary Ewan Park and contributors.
Cake is provided as-is under the MIT license. For more information see [LICENSE](https://github.com/cake-build/cake/blob/develop/LICENSE).

* For Roslyn, see https://github.com/dotnet/roslyn/blob/master/License.txt
* For Mono.CSharp, see https://github.com/mono/mono/blob/master/mcs/LICENSE
* For Autofac, see https://github.com/autofac/Autofac/blob/master/LICENSE
* For NuGet.Core, see https://nuget.codeplex.com/license

## Thanks

A big thank you has to go to [JetBrains](https://www.jetbrains.com) who provide each of the Cake Developers with an [Open Source License](https://www.jetbrains.com/support/community/#section=open-source) for [ReSharper](https://www.jetbrains.com/resharper/) that helps with the development of Cake.

The Cake Team would also like to say thank you to the guys at [MyGet](https://www.myget.org/) for their support in providing a Professional Subscription which allows us to continue to push all of our pre-release editions of Cake NuGet packages for early consumption by the Cake Community.

## Code of Conduct

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).

## Contribution License Agreement

By signing the [CLA](https://cla2.dotnetfoundation.org/), the community is free to use your contribution to .NET Foundation projects.

## .NET Foundation

This project is supported by the [.NET Foundation](http://www.dotnetfoundation.org).";
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Markdown Files (*.md)|*.md|All Files (*.*)|*.*";
            sfd.OverwritePrompt = true;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = sfd.FileName;
                File.WriteAllText(fileName, richTextBox1.Text);
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {

        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dirty)
            {
                File.WriteAllText(fileName, richTextBox1.Text);
                dirty = false;

                //switch (MessageBox.Show("Changes have not been saved.  Save now?", "Warning", MessageBoxButtons.YesNoCancel))
                //{
                //    case System.Windows.Forms.DialogResult.Yes:
                //        File.WriteAllText(fileName, richTextBox1.Text);
                //        break;
                //    case System.Windows.Forms.DialogResult.No:
                //        break;
                //    case System.Windows.Forms.DialogResult.Cancel:
                //        e.Cancel = true;
                //        break;
                //}
            }
        }

        bool dirty = false;

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            dirty = true;
            autoSaveTimer.Enabled = true;
            autoSaveTimer.Stop();   // in case the user is still typing
            autoSaveTimer.Start();
        }

        Timer autoSaveTimer;

        private void gitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = @"[https://guides.github.com/features/wikis/](https://guides.github.com/features/wikis/)
# Project name:
Your project’s name is the first thing people will see upon scrolling down to your README, and is included upon creation of your README file.

# Description:
A description of your project follows. A good description is clear, short, and to the point. Describe the importance of your project, and what it does.

# Table of Contents:
Optionally, include a table of contents in order to allow other people to quickly naviagte especially long or detailed READMEs.

# Installation:
Installation is the next section in an effective README. Tell other users how to install your project locally. Optionally, include a gif to make the process even more clear for other people.

# Usage:
The next section is usage, in which you instruct other people on how to use your project after they’ve installed it. This would also be a good place to include screenshots of your project in action.

# Contributing:
Larger projects often have sections on contributing to their project, in which contribution instructions are outlined. Sometimes, this is a separate file. If you have specific contribution preferences, explain them so that other developers know how to best contribute to your work. To learn more about how to help others contribute, check out the guide for (setting guidelines for repository contributors)[https://help.github.com/articles/setting-guidelines-for-repository-contributors/].

# Credits:
Include a section for credits in order to highlight and link to the authors of your project.

# License:
Finally, include a section for the license of your project. For more information on choosing a license, check out GitHub’s licensing guide!";

        }

        private void richTextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //if (e.Button == System.Windows.Forms.MouseButtons.Right)
            //{   //click event
            //    //MessageBox.Show("you got it!");
            //    ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            //    MenuItem menuItem = new MenuItem("Cut");
            //    menuItem.Click += new EventHandler(CutAction);
            //    contextMenu.MenuItems.Add(menuItem);
            //    menuItem = new MenuItem("Copy");
            //    menuItem.Click += new EventHandler(CopyAction);
            //    contextMenu.MenuItems.Add(menuItem);
            //    menuItem = new MenuItem("Paste");
            //    menuItem.Click += new EventHandler(PasteAction);
            //    contextMenu.MenuItems.Add(menuItem);

            //    richTextBox1.ContextMenu = contextMenu;
            //}
        }
        void CutAction(object sender, EventArgs e)
        {
            richTextBox1.Cut();
        }

        //void CopyAction(object sender, EventArgs e)
        //{
        //    Graphics objGraphics;
        //    Clipboard.SetData(DataFormats.Rtf, richTextBox1.SelectedRtf);
        //    Clipboard.Clear();
        //}

        //void PasteAction(object sender, EventArgs e)
        //{
        //    if (Clipboard.ContainsText(TextDataFormat.Rtf))
        //    {
        //        richTextBox1.SelectedRtf
        //            = Clipboard.GetData(DataFormats.Rtf).ToString();
        //    }
        //}

        void CopyAction(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.SelectedText);
        }

        void PasteAction(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                richTextBox1.Text
                    += Clipboard.GetText(TextDataFormat.Text).ToString();
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            System.Windows.Forms.HtmlDocument htmlDoc = webBrowser1.Document;
            try
            {
                htmlDoc = webBrowser1.Document;
                if (htmlDoc != null)
                {
                    htmlDoc.GetElementsByTagName("HTML")[0].ScrollTop = vPos;
                    htmlDoc.GetElementsByTagName("HTML")[0].ScrollLeft = hPos;
                }
            }
            catch (Exception ex)
            {
                vPos = 0;
                hPos = 0;
            }
        }

        public void Clear()
        {
            richTextBox1.Text = "";
            webBrowser1.DocumentText = "";
        }
    }

    public class Operations : Dictionary<string, Operation>
    {
        public void Add(string trigger, string prefix)
        {
            Add(trigger, prefix, string.Empty);
        }

        public void Add(string trigger, string prefix, string suffix)
        {
            base.Add(trigger, new Operation(trigger, prefix, suffix));
        }

        public void Add(string trigger, OperationDelegate prefix, string suffix)
        {
            base.Add(trigger, new Operation(trigger, prefix, suffix));
        }

        public void Add(string trigger, string prefix, OperationDelegate suffix)
        {
            base.Add(trigger, new Operation(trigger, prefix, suffix));
        }

        public void Add(string trigger, OperationDelegate prefix, OperationDelegate suffix)
        {
            base.Add(trigger, new Operation(trigger, prefix, suffix));
        }
    }

    public class Operation
    {
        string trigger = string.Empty;
        string prefix = string.Empty;
        string suffix = string.Empty;
        OperationDelegate prefixDelegate = null;
        OperationDelegate suffixDelegate = null;
        bool keepOriginal = true;

        public Operation(string trigger, string prefix)
        {
            this.trigger = trigger;
            this.prefix = prefix;
        }

        public Operation(string trigger, OperationDelegate prefix)
        {
            this.trigger = trigger;
            this.prefixDelegate = prefix;
        }

        public Operation(string trigger, string prefix, string suffix)
        {
            this.trigger = trigger;
            this.prefix = prefix;
            this.suffix = suffix;
        }

        public Operation(string trigger, OperationDelegate prefix, string suffix)
        {
            this.trigger = trigger;
            this.prefixDelegate = prefix;
            this.suffix = suffix;
        }

        public Operation(string trigger, string prefix, OperationDelegate suffix)
        {
            this.trigger = trigger;
            this.prefix = prefix;
            this.suffixDelegate = suffix;
        }

        public Operation(string trigger, OperationDelegate prefix, OperationDelegate suffix)
        {
            this.trigger = trigger;
            this.prefixDelegate = prefix;
            this.suffixDelegate = suffix;
        }

        public bool KeepOriginal
        {
            get
            {
                return this.keepOriginal;
            }
            set
            {
                this.keepOriginal = value;
            }
        }

        public OperationDelegate PrefixDelegate
        {
            get
            {
                return this.prefixDelegate;
            }
            set
            {
                this.prefixDelegate = value;
            }
        }

        public OperationDelegate SuffixDelegate
        {
            get
            {
                return this.suffixDelegate;
            }
            set
            {
                this.suffixDelegate = value;
            }
        }

        public string Trigger
        {
            get
            {
                return this.trigger;
            }
            set
            {
                this.trigger = value;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
            set
            {
                this.prefix = value;
            }
        }

        public string Suffix
        {
            get
            {
                return this.suffix;
            }
            set
            {
                this.suffix = value;
            }
        }
    }
}
