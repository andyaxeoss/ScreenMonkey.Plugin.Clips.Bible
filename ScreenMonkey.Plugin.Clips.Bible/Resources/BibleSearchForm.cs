#region Copyright © 2011 Oliver Waits, Andy Axe
//______________________________________________________________________________________________________________
//  
// Copyright © 2011 Oliver Waits, Andy Axe
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//______________________________________________________________________________________________________________
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace ScreenMonkey.Plugin.Clips.Bible.Resources
{
    public partial class BibleSearchForm : Form
    {
        public string StrTranslationFile = null; //must be set by plugin before loading Form!
        public string StrBookName;
        public int IntChapter;
        public int IntVerse;
        private static XmlNodeList xmlresults;
        private static string[] strresults;

        #region searchcode
        private void performsearch()
        {
            BackgroundWorker parseResults = new BackgroundWorker();
            progressBar1.Maximum = 100;
            progressBar1.Visible = true;
            parseResults.WorkerReportsProgress = true;
            parseResults.RunWorkerCompleted += new RunWorkerCompletedEventHandler(parseResults_RunWorkerCompleted);
            parseResults.ProgressChanged += new ProgressChangedEventHandler(parseResults_ProgressChanged);
            parseResults.DoWork += new DoWorkEventHandler(parseResults_DoWork);
            if (chkSearchWithin.Checked)
            {
                parseResults.DoWork -= parseResults_DoWork;
                parseResults.DoWork += new DoWorkEventHandler(parseResults_SearchWithin);
            }
            parseResults.RunWorkerAsync(xmlresults);
            lblProgress.Visible = true;
            lblProgress.Text = "Performing search...";
            return;
        }

        private void parseResults_SearchWithin(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker workersender = sender as BackgroundWorker;

            int intResults = lstResults.Items.Count;
            ArrayList strNewResults = new ArrayList();
            for(int counter=0;counter < intResults;counter++)
            {
                workersender.ReportProgress(( (counter+1) / intResults) * 100, "Processing results...");
                if(strresults[counter].Contains(txtSearchTerms.Text))
                {
                    strNewResults.Add(strresults[counter]);
                }
            }
            strresults = new string[strNewResults.Count];
            strresults = (string []) strNewResults.ToArray(typeof(string));
            workersender.ReportProgress(100, strNewResults.Count.ToString() + " results found.");
        }

        private void parseResults_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker workersender = sender as BackgroundWorker;
            XmlDocument xmlTranslation = new XmlDocument();
            xmlTranslation.Load(StrTranslationFile);
            XmlNode xmlRoot = xmlTranslation.DocumentElement;
            string strXPath = "//*[contains(text(),'" + txtSearchTerms.Text + "')]";
            if(chkCaseSensitivity.Checked)strXPath = "//*[contains(translate(text(),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')," + txtSearchTerms.Text.ToUpper() + "')]"; //Case Insensitive search, but does not work
            xmlresults = xmlRoot.SelectNodes(strXPath);
            if (xmlresults.Count == 0)
            {
                return;
            }
            workersender.ReportProgress(0,xmlresults.Count.ToString() + " results found.");
            int i=0;
            strresults = new string[xmlresults.Count];
            foreach (XmlNode xmlResult in xmlresults)
            {
                i++;
                workersender.ReportProgress( (i/xmlresults.Count)*100, "Processing result "+i.ToString()+" of "+xmlresults.Count.ToString()+" results" );
                int intTest;
                XmlAttributeCollection xmlVersenum = xmlResult.ParentNode.Attributes;
                string strChapter = xmlVersenum.Item(0).Value.ToString();
                xmlVersenum = xmlResult.Attributes;
                string strVerse = xmlVersenum.Item(0).Value.ToString();
                xmlVersenum = xmlResult.ParentNode.ParentNode.Attributes;
                string strBook = xmlVersenum.Item(0).Value.ToString();
                if (int.TryParse(strBook, out intTest))
                {
                    //this is zefania translation, have to search for bname attribute
                    strBook = xmlVersenum.GetNamedItem("bname").Value.ToString();
                }
                strresults[i - 1] = strBook + " " + strChapter + ":" + strVerse + " " + xmlResult.InnerText.ToString();
            }
            workersender.ReportProgress(100, xmlresults.Count.ToString() + " results found.");
        }

        private void parseResults_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            if (progressBar1.Value > 0) progressBar1.Visible = true; else progressBar1.Visible = false;
            lblProgress.Text = e.UserState.ToString();
        }

        private void parseResults_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            lstResults.DataSource = strresults;
            if (lstResults.Items.Count == 0 || xmlresults.Count == 0)
            {
                lblProgress.Visible = true;
                lblProgress.Text = "No results found, please try again!";
                lstResults.DataSource = null;
            }
            if (lstResults.Items.Count > 0)
            {
                lstResults.SelectedItem = lstResults.Items[0];
                lstResults.Focus();
            }
        }
        #endregion searchcode

        public BibleSearchForm()
        {
            InitializeComponent();
            lstResults.MultiColumn = false;
            lstResults.SelectionMode = SelectionMode.One;
            lstResults.HorizontalScrollbar = true;
            txtSearchTerms.KeyDown += new KeyEventHandler(txtSearchTerms_KeyDown);
            txtSearchTerms.KeyPress += new KeyPressEventHandler(txtSearchTerms_KeyPress);
            txtSearchTerms.Focus();
            lstResults.KeyDown += new KeyEventHandler(lstResults_KeyDown);
        }

        #region events
        private void txtSearchTerms_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)13) e.Handled = true; //disable beep on enter keypress
        }

        private void lstResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                this.DialogResult = DialogResult.OK;
                btnSelect_Click(sender, e);
            }
        }

        private void txtSearchTerms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter )
            {
                e.Handled = true;
                lblProgress.Visible = false;
                performsearch();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            lblProgress.Visible = false;
            performsearch();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            string strSelectedresult;
            if (lstResults.SelectedItems.Count > 0)
            {
                strSelectedresult = lstResults.SelectedItem.ToString(); //Get the selected entry from list box
            }
            else return;

            //Now parse the string to get the selected verse
            string[] strVerseinfo = strSelectedresult.Split(':'); //first part should be book and chapter, second be verse & rest
            string[] strBookchap = strVerseinfo[0].Split(' '); // last section should be chapter, rest is book
            IntChapter = Convert.ToInt32(strBookchap[strBookchap.GetUpperBound(0)]); //last element of array in book & chap string
            string[] strVerseref = strVerseinfo[1].Split(' '); //strVerseref[0] should be verse
            IntVerse = Convert.ToInt32(strVerseref[0]);
            StrBookName = "";
            for (int i = 0; i < strBookchap.GetUpperBound(0); i++)
            {
                if(i==0 || strBookchap[i]!=IntChapter.ToString())StrBookName += strBookchap[i]; //This loop is done to account for spaces in book names
                if(i==0 && strBookchap.GetUpperBound(0)!=1 )StrBookName += " "; //Add necessary spaces back in for book name
            }
            this.Close();
        }
        #endregion events
    }
}
