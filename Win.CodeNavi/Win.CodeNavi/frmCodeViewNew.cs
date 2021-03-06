﻿/*
Released as open source by NCC Group Plc - http://www.nccgroup.com/

Developed by Ollie Whitehouse, ollie dot whitehouse at nccgroup dot com

http://www.github.com/nccgroup/ncccodenavi

Released under AGPL see LICENSE for more information
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ScintillaNET;
using System.Collections;

namespace Win.CodeNavi
{
    public partial class frmCodeViewNew : Form
    {
        private string strFilePath = null;
        private int intLine = 0;
        private frmMain frmMaster = null;
        private bool _iniLexer;
        private string _filePath;
        private Stack stackRanges = new Stack();

        public bool IniLexer
        {
            get { return _iniLexer; }
            set { _iniLexer = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddOrRemoveAsteric()
        {
            if (scintilla.Modified)
            {
                if (!Text.EndsWith(" *"))
                    Text += " *";
            }
            else
            {
                if (Text.EndsWith(" *"))
                    Text = Text.Substring(0, Text.Length - 2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scintilla_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            // Style the _text
            if (_iniLexer)
                Win.CodeNavi.IniLexer.StyleNeeded((Scintilla)sender, e.Range);
        }

        public Scintilla Scintilla
        {
            get
            {
                return scintilla;
            }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        // 
        // http://social.msdn.microsoft.com/forums/en-US/csharpgeneral/thread/5bbac0e3-437e-495e-9680-b8349ec5f428
        //
        private void Goto(Scintilla myRichTextBox, Int32 lineToGo)
        {

            Int32 intLine = 0;
            Int32 intNineLinesPast = 0;
            Int32 intTwoLinesPast = 0;
            String text = myRichTextBox.Text;

            if (lineToGo > myRichTextBox.Lines.Count) lineToGo = myRichTextBox.Lines.Count;

            for (Int32 i = 1; i < lineToGo; i++)
            {
                intLine = text.IndexOf('\n', intLine + 1);
                if (intLine == -1) break;
            }

            for (Int32 i = 1; i < lineToGo + 9; i++)
            {
                intNineLinesPast = text.IndexOf('\n', intNineLinesPast + 1);
                if (intNineLinesPast == -1) break;
            }

            for (Int32 i = 1; i < lineToGo + 2; i++)
            {
                intTwoLinesPast = text.IndexOf('\n', intTwoLinesPast + 1);
                if (intTwoLinesPast == -1) break;
            }

            try
            {

                if (lineToGo > 1 && intNineLinesPast != -1)
                {
                    myRichTextBox.GoTo.Position(intNineLinesPast);
                    myRichTextBox.Lines[lineToGo-1].AddMarker(0);
                    myRichTextBox.GoTo.Line(lineToGo-1);
                }
                else if (lineToGo > 1 && intTwoLinesPast != -1)
                {
                    myRichTextBox.GoTo.Position(intTwoLinesPast);
                    myRichTextBox.Lines[lineToGo-1].AddMarker(0);
                    myRichTextBox.GoTo.Line(lineToGo-1);
                }
                else if (lineToGo > 1 && intLine != -1)
                {
                    myRichTextBox.GoTo.Position(intLine);
                    myRichTextBox.Lines[lineToGo-1].AddMarker(0);
                }
                else
                {
                    myRichTextBox.GoTo.Position(intLine);
                }
            }
            catch (Exception)
            {

            }

        }


        public frmCodeViewNew()
        {
            InitializeComponent(); 
        }

        public frmCodeViewNew(String strFile,int intLine, frmMain frmMaster)
        {
            InitializeComponent();
            this.Text = "Code View - " + strFile;
            strFilePath = strFile;
            this.intLine = intLine;
            this.frmMaster = frmMaster;
            this.scintilla.Styles[1].BackColor = Color.LightGreen;
            this.scintilla.Styles[1].ForeColor = Color.White;
            this.scintilla.Styles[1].Font = new Font(this.scintilla.Font, FontStyle.Regular);
            this.scintilla.Indicators[0].Style = IndicatorStyle.RoundBox;
            this.scintilla.Indicators[0].Color = Color.Green;
        }

        private void frmCodeViewNew_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Escape))
            {
                if (MessageBox.Show("Are you sure you wish to close this file?", "Are you sure?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    //this.Visible = false;
                    this.Close();
                }
            }
            else if (e.Control && e.KeyCode.ToString().Equals("W"))
            {
                if (MessageBox.Show("Are you sure you wish to close this file?", "Are you sure?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    //this.Visible = false;
                    this.Close();
                }
            }
            else if (e.KeyCode.Equals(Keys.Enter)) // this is a terrible hack as the onKeyPress events didn't fire in the Scintilla events
            {
                if (this.Scintilla.Selection.Length > 0)
                {
                    frmMaster.DoSearchFromCode(Scintilla.Selection.Text.TrimEnd(' '));
                    e.Handled = true;
                }
            }
        }
        
        private const int LINE_NUMBERS_MARGIN_WIDTH = 35; // TODO Don't hardcode this

        private void frmCodeViewNew_Load(object sender, EventArgs e)
        {
            if (File.Exists(strFilePath) == false)
            {
                MessageBox.Show("File not found " + strFilePath, "File not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                return;
            }

            this.Scintilla.Text = File.ReadAllText(strFilePath);
            

            try
            {
                Console.WriteLine(frmMain.AssemblyDirectory + "\\NCCCodeNavi.CodeHighlighting\\" + Path.GetExtension(strFilePath).Substring(1) + ".xml");
                if(File.Exists(frmMain.AssemblyDirectory + "\\NCCCodeNavi.CodeHighlighting\\" + Path.GetExtension(strFilePath).Substring(1) + ".xml")){
                    this.Scintilla.ConfigurationManager.IsBuiltInEnabled = false;
                    this.Scintilla.ConfigurationManager.CustomLocation = frmMain.AssemblyDirectory + "\\NCCCodeNavi.CodeHighlighting\\";
                    this.scintilla.ConfigurationManager.Language = "default";
                    scintilla.Lexing.LexerLanguageMap[Path.GetExtension(strFilePath).Substring(1)] = "cpp"; // probably a bit too dirty in the long term
                    this.Scintilla.ConfigurationManager.Language = Path.GetExtension(strFilePath).Substring(1);
                    if (Path.GetExtension(strFilePath).Substring(1).ToLower().Equals("cs")) this.Scintilla.Indentation.SmartIndentType = SmartIndent.CPP;
                    if (Path.GetExtension(strFilePath).Substring(1).ToLower().Equals("c")) this.Scintilla.Indentation.SmartIndentType = SmartIndent.CPP;
                } else {
                    if (Path.GetExtension(strFilePath).Substring(1).ToLower().Equals("cs")) this.Scintilla.Indentation.SmartIndentType = SmartIndent.CPP;
                    if (Path.GetExtension(strFilePath).Substring(1).ToLower().Equals("c"))  this.Scintilla.Indentation.SmartIndentType = SmartIndent.CPP;
                    this.Scintilla.ConfigurationManager.IsBuiltInEnabled = true;
                    this.Scintilla.ConfigurationManager.Configure();
                }
                
            }
            catch (Exception)
            {
                this.Scintilla.ConfigurationManager.IsBuiltInEnabled = true;
                if (Path.GetExtension(strFilePath).Substring(1).ToLower().Equals("cs")) this.Scintilla.Indentation.SmartIndentType = SmartIndent.CPP;
                if (Path.GetExtension(strFilePath).Substring(1).ToLower().Equals("c")) this.Scintilla.Indentation.SmartIndentType = SmartIndent.CPP;
                this.Scintilla.ConfigurationManager.Language = Path.GetExtension(strFilePath).Substring(1);
                this.Scintilla.ConfigurationManager.Configure();
            }

            this.IniLexer = false;
            this.Scintilla.UndoRedo.EmptyUndoBuffer();
            this.Scintilla.Modified = false;
            this.FilePath = strFilePath;
            this.Scintilla.IsReadOnly = true;
            this.Scintilla.Modified = false;
            this.Scintilla.Margins.Margin0.Width = LINE_NUMBERS_MARGIN_WIDTH;

            Goto(Scintilla, intLine);
          
            this.Select();
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Scintilla.Selection.Text.Length > 0)
            {
                frmMaster.DoSearchFromCode(Scintilla.Selection.Text.TrimEnd(' '));
            }
        }

        private void googleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Scintilla.Selection.Text != null && this.Scintilla.Selection.Text.Length > 0)
            {
                string strTarget = "http://www.google.com/search?q=" + Scintilla.Selection.Text.TrimEnd(' ');

                try
                {
                    System.Diagnostics.Process.Start(strTarget);
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    if (noBrowser.ErrorCode == -2147467259) MessageBox.Show(noBrowser.Message);
                }
                catch (System.Exception other)
                {
                    MessageBox.Show(other.Message);
                }
            }
		

        }

        private void frmCodeViewNew_Shown(object sender, EventArgs e)
        {

        }

        private void cmdCERTSearch_Click(object sender, EventArgs e)
        {
            if (this.Scintilla.Selection.Text != null && this.Scintilla.Selection.Text.Length > 0)
            {
                string strTarget = "http://search.cert.org/search?client=default_frontend&site=default_collection&output=xml_no_dtd&proxystylesheet=default_frontend&ie=UTF-8&oe=UTF-8&as_q=" + Scintilla.Selection.Text.TrimEnd(' ') + "&num=10&btnG=Search&as_epq=&as_oq=&as_eq=&lr=&as_ft=i&as_filetype=&as_occt=any&as_dt=i&as_sitesearch=www.securecoding.cert.org&sort=&as_lq=";

                try
                {
                    System.Diagnostics.Process.Start(strTarget);
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    if (noBrowser.ErrorCode == -2147467259) MessageBox.Show(noBrowser.Message);
                }
                catch (System.Exception other)
                {
                    MessageBox.Show(other.Message);
                }
            }
        }

        private void ctxCodeView_Opening(object sender, CancelEventArgs e)
        {

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.Scintilla.Selection.Text != null && this.Scintilla.Selection.Text != "") System.Windows.Forms.Clipboard.SetText(this.Scintilla.Selection.Text);
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            searchToolStripMenuItem_Click(null, null);
        }

        private void cmdGoogle_Click(object sender, EventArgs e)
        {
            googleToolStripMenuItem_Click(null, null);
        }

        private void cmdCERT_Click(object sender, EventArgs e)
        {
            cmdCERTSearch_Click(null, null);
        }

        private void cmdCopy_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(null, null);
        }

        private void HandleSelectionChange(object sender, EventArgs e)
        {
            if (scintilla.Selection.Text.Length == 0) return;
            // http://scintillanet.codeplex.com/discussions/53292
            this.scintilla.NativeInterface.IndicatorClearRange(0, this.scintilla.Text.Length);

            // http://scintillanet.codeplex.com/discussions/361003
            foreach (Range r in scintilla.FindReplace.FindAll(scintilla.Selection.Text))
            {
                r.SetIndicator(0);
            }
        }


        private void cmdSendFileNamePathToNotes_Click(object sender, EventArgs e)
        {
            StringBuilder strNote = new StringBuilder();
            string strTmp = this.Text.Substring(this.Text.IndexOf("-")+1).TrimStart();

            strNote.Append(strTmp + Environment.NewLine);
            frmMaster.SendToNotes(strNote.ToString());
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void cmdBookmark_Click(object sender, EventArgs e)
        {
            Line currentLine = this.Scintilla.Lines.Current;
            if (this.Scintilla.Markers.GetMarkerMask(currentLine) == 0)
            {
                currentLine.AddMarker(0);
            }
            else
            {
                currentLine.DeleteMarker(0);
            }
        }

        private void cmdPreviousBookMark_Click(object sender, EventArgs e)
        {
            Line l = this.Scintilla.Lines.Current.FindPreviousMarker(1);
            if (l != null) l.Goto();
        }

        private void cmdNextBookmark_Click(object sender, EventArgs e)
        {
            Line l = this.Scintilla.Lines.Current.FindNextMarker(1);
            if (l != null) l.Goto();
        }

        private void cmdDeleteBookmarks_Click(object sender, EventArgs e)
        {
            this.Scintilla.Markers.DeleteAll(0);
        }
 
    }
}
