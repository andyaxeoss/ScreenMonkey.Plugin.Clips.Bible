#region Copyright © 2011-2021 Oliver Waits, Andy Axe, online community
//______________________________________________________________________________________________________________
//  
// Copyright © 2011 Oliver Waits, Andy Axe, online community
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

using System.Windows.Forms;  //for openfiledialog
using System.Globalization; //for invariant culture to save to config files
using System.Collections;
using System.Collections.Generic; //for getting lists
using System.Drawing; //to draw controls...
using System;
using System.IO;
using ScreenMonkey.Clips; //basic screenmonkey clip functionality
using System.Xml; //for loading/saving files
using ScreenMonkey.Clips.Extensions; //to get extra interfaces, like right click menus...
using ScreenMonkey.Text;

namespace ScreenMonkey.Plugin.Clips.Bible
{

    public class BibleClip : ClipMediaBase, IMenuSettings, ICustomPaint, ICueClip, ISlidePlayback
    {
        ///<summary>
        ///This Clip Loads and Displays Opensong and Zefania Formatted Bibles Within Screen Monkey. 
        /// </summary>

        #region variables
        private ToolStripMenuItem loadnewtranslationfile = new ToolStripMenuItem("Load New Translation File");
        private ToolStripMenuItem showdisplayoptions = new ToolStripMenuItem("Display Options");
        private WordsDisplay mainoutput = null;
        private System.Windows.Forms.Panel livemonitor = null;
        private System.Windows.Forms.Label lblversepreview = null;
        private System.Windows.Forms.Label lblstartverse = null;
        // private Label lblEndVerse = null;
        private Bible bible = new Bible();
        //  private ArrayList VerseSelection = null;
        private System.Windows.Forms.Label lbltheverse = new System.Windows.Forms.Label();
        private Button btnnext = null;
        private Button btnprevious = null;
        private Button btnsearch = null;
        private System.Windows.Forms.ComboBox cmbstartbook;//, endbook;
        private System.Windows.Forms.ComboBox cmbstartchapter;//, endchapter;
        private System.Windows.Forms.ComboBox cmbstartverse;//, endverse;
        private CheckBox chkshownumbers,chkshowtitle;
        private System.Windows.Forms.Button btnshowdisplayoptions;

        public BibleDisplayStyle DisplayStyle = new BibleDisplayStyle();
        
        public int intSavedStartingBook = 0;
        public int intSavedStartingChapter = 0;
        public int intSavedStartingVerse = 0;

        public String StrTranslationFileName = null;
        public bool boolShowVerseNumbers = true;
        public bool boolShowPassageTitle = false;
        public event EventHandler Invalidated;
        #endregion


        public BibleClip()
        {
            loadnewtranslationfile.Click += new EventHandler(CueFile);
            showdisplayoptions.Click += new EventHandler(showdisplayoptions_Click);
            buildmainoutput();
           
        }

        private void buildmainoutput()
        {
            mainoutput = new WordsDisplay();
            mainoutput.DrawInvalidated += new EventHandler(mainOutput_DrawInvalidated);
            mainoutput.DisplayStyle = DisplayStyle;
            return;
        }

        private void mainOutput_DrawInvalidated(object sender, EventArgs e)
        {
            if (Invalidated != null) Invalidated(sender, e);
        }

        #region ISlidePlayback

        public bool AutoRewind //just thrown in to implement ISlidePlayback - not actually used
        {
            get { return false; }
            set { return; }
        }

        public ISlideCuePoint CurrentSlide //just thrown in to implement ISlidePlayback - not actually used
        {
            get
            {
                return null;
            }
            set
            {
                return; 
            } 
        }
        
        public bool LoopSlides //just thrown in to implement ISlidePlayback - not actually used
        {
            get
            {
                return false;
            }
            set
            {
                return; 
            } 
        }

        public int SlideCount { get { return 0; } } //not implemented

        public int SlideIndex { get {return bible.IntSelectedVerse;} }

        public List<ISlideInfo> Slides { get {return null;} } //not used, just here to have it implemented

        public ISlideCuePoint StartTime { get { return null; } set { return; } } //again, not used, just here for implementation

        public ISlideCuePoint StopTime { get { return null; } set { return; } } //again, not used, just here for implementation

        public int SubSlideIndex { get { return 0; } }

        public int SubSlideCount(int slideindex)
        {
            return 0;
        }

        public void RewindSlides() { return; }

        public void GotoSlide(int index) { return; }

        public void NextSlide() { Next_Click(this, new EventArgs()); }

        public void PreviousSlide() { Prev_Click(this, new EventArgs()); }

        #endregion

        #region Repair Clip

        public override IRepairInformation Repair(IRepairInformation info)
        {
            RepairInformation check = null;
            if (info is RepairInformation)
            {
                check = (RepairInformation)info;
                StrTranslationFileName = GetTranslationFileName(StrTranslationFileName);
                Load();
            }
            else
            {
                StrTranslationFileName = null;
                StrTranslationFileName = GetTranslationFileName(StrTranslationFileName);
                if (File.Exists(StrTranslationFileName))
                {
                    Load();
                    check = new RepairInformation();
                    check.Repaired = true;
                    check.RepairAll = true;
                    check.NewPath = StrTranslationFileName;
                }
            }
           
            return check;
        }

        #endregion

        #region ICustomPaint
        public bool AlphaRequired()
        {
            return DisplayStyle.BackgroundTransparent;
        }

        public void Paint(System.Drawing.Bitmap bmpcanvas, System.Drawing.Rectangle rectArea, System.Drawing.Rectangle rectPanArea)
        {
            if (mainoutput != null) mainoutput.Draw(bmpcanvas, rectPanArea);
            return;
        }
        #endregion

        #region File Loading & Saving

        protected override void OnSaveSettings(XmlWriter xmlSettingsFile, ClipSaveOptions xmlPackage)
        {
            if (xmlPackage.Package == true)
            {
                //int offset;
                StrTranslationFileName = "packaged";//ClipTempFolder.ToString() + /*Path.DirectorySeparatorChar +*/ "temptranslation.xml";
                //package necessary files here...
                //XmlWriter tmp = XmlWriter.Create(translationfilename);
               // bible.SaveXML(settingsfile);
               // tmp.Close();
                xmlSettingsFile.WriteStartElement("PackageOffset");
                //offset = bible.BookOffset+1;// -1;
                xmlSettingsFile.WriteAttributeString("book", intSavedStartingBook.ToString(CultureInfo.InvariantCulture));
                //offset = bible.ChapterOffset+1;// -1;
                xmlSettingsFile.WriteAttributeString("chapter", intSavedStartingChapter.ToString(CultureInfo.InvariantCulture));
                //offset = bible.VerseOffset+1;// -1;
                xmlSettingsFile.WriteAttributeString("verse", intSavedStartingVerse.ToString(CultureInfo.InvariantCulture));
                xmlSettingsFile.WriteEndElement();
                bible.IntBookOffset = 0;
                bible.IntChapterOffset = 0;
                bible.IntVerseOffset = 0;
            }
            xmlSettingsFile.WriteElementString("translation", StrTranslationFileName);
            xmlSettingsFile.WriteElementString("startbook", intSavedStartingBook.ToString(CultureInfo.InvariantCulture));
            xmlSettingsFile.WriteElementString("startchapter", intSavedStartingChapter.ToString(CultureInfo.InvariantCulture));
            xmlSettingsFile.WriteElementString("startverse", intSavedStartingVerse.ToString(CultureInfo.InvariantCulture));
            xmlSettingsFile.WriteElementString("showversenumbers", boolShowVerseNumbers.ToString(CultureInfo.InvariantCulture));
            xmlSettingsFile.WriteElementString("showpassagetitle", boolShowPassageTitle.ToString(CultureInfo.InvariantCulture));
            DisplayStyle.SaveXml(xmlSettingsFile, null); //Do we need ConformPathHandler?
        }

        protected override void OnPackageShow(XmlWriter xml)
        {
            base.OnPackageShow(xml);
            bible.SaveXML(xml);
            if(!DisplayStyle.BackgroundTransparent && !String.IsNullOrEmpty(DisplayStyle.BackgroundImage) ) PackageFile(xml, new FileInfo(DisplayStyle.BackgroundImage));
        }

        protected override void OnUnpackageShow(XmlNode xml)
        {
            base.OnUnpackageShow(xml);
            UnpackageFiles(xml);
            bible.Load(xml);
        }
        
        protected override void OnLoadSettings(XmlNode xmlSettings)
        {
            UnpackageFiles(xmlSettings);
            try
            {
                XmlNode xmlInfotoCheck = xmlSettings.SelectSingleNode("translation/text()");
                if (xmlInfotoCheck != null)
                {
                    StrTranslationFileName = xmlInfotoCheck.Value; //load translation file path
                }
                else StrTranslationFileName = null;
                xmlInfotoCheck = xmlSettings.SelectSingleNode("startbook/text()");
                if (xmlInfotoCheck != null)
                {
                    intSavedStartingBook = Convert.ToInt32(xmlInfotoCheck.Value);//load starting book
                }
                else intSavedStartingBook = 0;
                xmlInfotoCheck = xmlSettings.SelectSingleNode("startchapter/text()");
                if (xmlInfotoCheck != null)
                {
                    intSavedStartingChapter = Convert.ToInt32(xmlInfotoCheck.Value);//load starting chapter
                }
                else intSavedStartingChapter = 0;
                xmlInfotoCheck = xmlSettings.SelectSingleNode("startverse/text()");
                if (xmlInfotoCheck != null)
                {
                    intSavedStartingVerse = Convert.ToInt32(xmlInfotoCheck.Value);//load starting verse
                }
                else intSavedStartingVerse = 0;
                xmlInfotoCheck = xmlSettings.SelectSingleNode("showversenumbers/text()");
                if (xmlInfotoCheck != null)
                {
                    boolShowVerseNumbers = Convert.ToBoolean(xmlInfotoCheck.Value);//load whether to show verse numbers
                }
                else boolShowVerseNumbers = true;
                xmlInfotoCheck = xmlSettings.SelectSingleNode("showpassagetitle/text()");
                if (xmlInfotoCheck != null)
                {
                    boolShowPassageTitle = Convert.ToBoolean(xmlInfotoCheck.Value);//load whether to show passage title
                }
                else boolShowPassageTitle = true;
                xmlInfotoCheck = xmlSettings.SelectSingleNode("PackageOffset/@book");
                if (xmlInfotoCheck != null)
                {
                    bible.IntBookOffset = Convert.ToInt32(xmlInfotoCheck.Value);//load book offset for packaging
                }
                else bible.IntBookOffset = 0;
                xmlInfotoCheck = xmlSettings.SelectSingleNode("PackageOffset/@chapter");
                if (xmlInfotoCheck != null)
                {
                    bible.IntChapterOffset = Convert.ToInt32(xmlInfotoCheck.Value);//load chapter offset for packaging
                }
                else bible.IntChapterOffset = 0;
                xmlInfotoCheck = xmlSettings.SelectSingleNode("PackageOffset/@verse");
                if (xmlInfotoCheck != null)
                {
                    bible.IntVerseOffset = Convert.ToInt32(xmlInfotoCheck.Value);//load verse offset for packaging
                }
                else bible.IntVerseOffset = 0;
                DisplayStyle.LoadXml(xmlSettings);
                
                if (!string.IsNullOrEmpty(StrTranslationFileName))
                {
                    StrTranslationFileName = ResolvePath(StrTranslationFileName).FullName;
                }

            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("Could not load translation file path from show file.");
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        private void setupdisplaystyle()
        {
            mainoutput.DisplayStyle.BackgroundColour = DisplayStyle.BackgroundColour;
            mainoutput.DisplayStyle.BackgroundTransparent = DisplayStyle.BackgroundTransparent;
            mainoutput.DisplayStyle.BackgroundImage = DisplayStyle.BackgroundImage;
        }

        #endregion

        #region Clip Loading

        protected override void OnLoadClip()
        {
            bible.LoadTranslationFile(StrTranslationFileName);
            UpdateThumbnailImage();
        }

        #endregion

        #region Event Functions
        
        protected void CueFile(object x, System.EventArgs y)
        {
            StrTranslationFileName = null;
            StrTranslationFileName = GetTranslationFileName(StrTranslationFileName);
            bible.BoolTranslationChanged = true;
            Load();
        }

        protected void Next_Click(object sender, EventArgs e) 
        {
            int intSelectedVerse = cmbstartverse.SelectedIndex;
            int intSelectedChapter = cmbstartchapter.SelectedIndex;
            int intSelectedBook = cmbstartbook.SelectedIndex;
            int intTotalVerses = bible.NewVerseCount(intSelectedBook, intSelectedChapter);
            intSelectedVerse++;
            if (intSelectedVerse > intTotalVerses-1) //if end of chapter, then go to next chapter
            {
                intSelectedChapter++;
                intSelectedVerse = 0;
                int intTotalChapters = bible.NewChapterCount(intSelectedBook);
                if (intSelectedChapter > intTotalChapters-1)
                {
                    intSelectedBook++;
                    if (intSelectedBook > bible.Books.Count-1) return; //at end of bible, so kick out - can't go further!
                    intSelectedChapter = 0;
                }
            }
            cmbstartbook.SelectedIndex = intSelectedBook;
            startbook_SelectionChangeCommitted(sender, e); //update book selection, so chapter combobox updates
            cmbstartchapter.SelectedIndex = intSelectedChapter;
            startchapter_SelectionChangeCommitted(sender, e); //update chapter selection, so verse combobox updates
            cmbstartverse.SelectedIndex = intSelectedVerse;
            startverse_SelectionChangeCommitted(sender, e);
            UpdateThumbnailImage();
            return;
        }

        protected void Prev_Click(object sender, EventArgs e) 
        {
            try
            {

                int intSelectedVerse = cmbstartverse.SelectedIndex;
                int intSelectedChapter = cmbstartchapter.SelectedIndex;
                int intSelectedBook = cmbstartbook.SelectedIndex;
                if (intSelectedBook == 0 && intSelectedChapter == 0 && intSelectedVerse == 0) return; //at beginning of bible, so kick out
                intSelectedVerse--;
                if (intSelectedVerse < 0 && intSelectedChapter > 0) //if beginning of chapter, then go to previous chapter
                {
                  intSelectedChapter--;
                  int intTotalVerses = bible.NewVerseCount(intSelectedBook, intSelectedChapter)-1; 
                  if (intTotalVerses < 0) throw new Exception("NewVerseCount returned no verses from Prev_Click()");
                  intSelectedVerse = intTotalVerses;
                }
                if (intSelectedVerse < 0 && intSelectedChapter == 0) //if beginning of book then go to previous book
                {
                  intSelectedBook--;
                  if (intSelectedBook < 0) return; //can't go to previous book if you are at the beginning already!
                  int intTotalChapters = bible.NewChapterCount(intSelectedBook)-1;
                  if (intTotalChapters < 0) throw new Exception("getChapterCount returned no chapters from Prev_Click()");
                  intSelectedChapter = intTotalChapters;//bible.getChapterCount(selectedbook)-1;
                  intSelectedVerse = bible.NewVerseCount(intSelectedBook, intSelectedChapter) - 1;
                  if (intSelectedVerse < 0) throw new Exception("NewVerseCount returned no verses from Prev_Click()");
                }

                cmbstartbook.SelectedIndex = intSelectedBook;
                startbook_SelectionChangeCommitted(sender, e); //update book selection, so chapter combobox updates*/
                cmbstartchapter.SelectedIndex = intSelectedChapter;
                startchapter_SelectionChangeCommitted(sender, e); //update chapter selection, so verse combobox updates
                cmbstartverse.SelectedIndex = intSelectedVerse;
                startverse_SelectionChangeCommitted(sender, e); //update verse selection to display on screen*/
                UpdateThumbnailImage();
                return;
            }
            catch(Exception exception)
            {
                throw exception;
            }
        }

        protected void startbook_GotFocus(object sender, EventArgs e)
        {
            cmbstartbook.SelectAll();
        }

        protected void startbook_LostFocus(object sender, EventArgs e)
        {
            startbook_SelectionChangeCommitted(sender, e);
            cmbstartchapter.SelectAll();
            cmbstartchapter.Focus();
        }

        protected void startbook_SelectionChangeCommitted(object sender, EventArgs e) 
        {   
            int intTotalBooks = bible.Books.Count;

            int intTotalChapters = bible.NewChapterCount(cmbstartbook.SelectedIndex);
            int intPreviousChapSelection=cmbstartchapter.SelectedIndex;
            cmbstartchapter.Items.Clear();
            for (int x = 0; x < intTotalChapters; x++)
            {
                int y = x + 1;
                cmbstartchapter.Items.Add(y);
            }
            if(cmbstartchapter.Items.Count < intPreviousChapSelection)cmbstartchapter.SelectedIndex = 0;
            intSavedStartingBook = cmbstartbook.SelectedIndex;
            bible.IntSelectedBook = intSavedStartingBook + 1;
            UpdateThumbnailImage();
            return;
        }
        /*
     protected void endbook_SelectionChangeCommitted(object sender, EventArgs e)
     {
       int totalbooks = Bible.Count;
       int startat=0;
       Book bookenum = (Book)Bible[startbook.SelectedIndex + endbook.SelectedIndex];
       int totalchapters = bookenum.chapters.Count;
       chapteroffset = startchapter.SelectedIndex;
       if (endbook.SelectedIndex !=  0)
       {
           startat = 0;
       }
       else
       {
           startat = chapteroffset;
       }
       endchapter.Items.Clear();       
       for (int x = startat; x < totalchapters; x++)
       {
           Chapter tmpchapter = (Chapter)bookenum.chapters[x];
           endchapter.Items.Add(tmpchapter.index.ToString());
       }
       endchapter.SelectedIndex = 0;
       return;
     }

     protected void endchapter_SelectionChangeCommitted(object sender, EventArgs e)
     {
       Book tmpbook = (Book)Bible[startbook.SelectedIndex+endbook.SelectedIndex];
       Chapter chapenum = (Chapter)tmpbook.chapters[startchapter.SelectedIndex + endchapter.SelectedIndex];
       endverse.Items.Clear();
       int startingverse;
       if (endchapter.SelectedIndex == 0)
       {
           startingverse = startverse.SelectedIndex;
       }
       else startingverse = 0;
       int totalverses = chapenum.verse.Count;
       for (int i = startingverse; i < totalverses; i++)
       {
           Verse tmp = (Verse)chapenum.verse[i];
           endverse.Items.Add(tmp.index);
       }
       endverse.SelectedIndex = 0;
       return;
     }
     */
        protected void startchapter_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (Convert.ToInt32(cmbstartchapter.Text) < 0) cmbstartchapter.SelectedIndex = 0;
            if (Convert.ToInt32(cmbstartchapter.Text) > (cmbstartchapter.Items.Count - 1)) cmbstartchapter.SelectedIndex = cmbstartchapter.Items.Count - 1;
            int intPreviousSelectedVerse = cmbstartverse.SelectedIndex;
            cmbstartverse.Items.Clear();

            int intTotalVerses = bible.NewVerseCount(cmbstartbook.SelectedIndex, cmbstartchapter.SelectedIndex);/*chapenum.verse.Count;*/
            if (intTotalVerses == 0) throw new Exception("totalverses returned 0 from NewVerseCount in startchapter selection event");
            for (int i = 0; i < intTotalVerses; i++)
            {
                int j = i + 1;
                cmbstartverse.Items.Add(j);
            }
            if(cmbstartverse.Items.Count < intPreviousSelectedVerse) cmbstartverse.SelectedIndex = 0;
            intSavedStartingChapter = cmbstartchapter.SelectedIndex;
            bible.IntSelectedChapter = intSavedStartingChapter + 1;
            UpdateThumbnailImage();
            cmbstartverse.SelectAll();
            return;
        }

        protected void startverse_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (Convert.ToInt32(cmbstartverse.Text) < 0) cmbstartverse.SelectedIndex = 0;
            if (Convert.ToInt32(cmbstartverse.Text) > (cmbstartverse.Items.Count - 1)) cmbstartverse.SelectedIndex = cmbstartverse.Items.Count - 1;
            String strtheVerse=null;
            if (boolShowVerseNumbers) strtheVerse = (cmbstartverse.SelectedIndex + 1).ToString()+" ";
            strtheVerse+= bible.NewGetVerse(cmbstartbook.SelectedIndex, cmbstartchapter.SelectedIndex, cmbstartverse.SelectedIndex);
            lblversepreview.Text = strtheVerse;
            intSavedStartingVerse = cmbstartverse.SelectedIndex;
            bible.IntSelectedVerse = intSavedStartingVerse + 1;
            if (this.Active)//if clip is playing
            {
                mainoutput.MainWords = strtheVerse;
                if (boolShowPassageTitle)
                {
                    mainoutput.SetSectionText("Passage Title", SetPassageTitle()); //Set Title of Passage
                }
                else mainoutput.SetSectionText("Passage Title", "");
                mainoutput.SetSectionText("Copyright", bible.StrCopyrightInfo);
                this.Invalidated(sender, e); //force redraw of mainOutput if it is available
            }
            UpdateThumbnailImage();
            return;
        }
        /*
           protected void endverse_SelectionChangeCommitted(object sender, EventArgs e)
           {
               //Need to calculate verses to add to text from start to finish...
               //And compensate for number of verses to show...

               //TODO: Amount of chapters to calculate is not working properly - comes up with wrong amount.

               VerseSelection = new ArrayList();
               Book tmpbook; //endselectionbook
               Chapter tmpchapter = null;
               int versestocheck = 1;
               int versestoadd = 0;
               int chapterstocheck;
               int startingchapter=0;
               int startingverse;

               if (endbook.SelectedIndex == 0) //start and end books are same
               {
                   chapterstocheck = endchapter.SelectedIndex+1;
                   if (endchapter.SelectedIndex == 0) //means start & end chapters are same
                   {
                       chapterstocheck = 1;
                       versestocheck = endverse.SelectedIndex+1;
                       if (endverse.SelectedIndex == 0) //same verse...
                       {
                           versestoadd = 0;
                           versestocheck = 1;
                       }
                   }
               }
               for (int x = startbook.SelectedIndex; x <= (startbook.SelectedIndex + endbook.SelectedIndex); x++)
               {
                   tmpbook = (Book) Bible[x];
                   //calculate proper amount of chapters to check for given book...
                   if (endbook.SelectedIndex == 0) //meaning same start and end book
                   {
                       //look at what was selected to determine chapters to check
                       startingchapter = startchapter.SelectedIndex;
                       chapterstocheck = startchapter.SelectedIndex + endchapter.SelectedIndex;
                   }
                   else if(x==(startbook.SelectedIndex+endbook.SelectedIndex))
                   {
                       startingchapter = 0;
                       chapterstocheck = endbook.SelectedIndex;
                   }
                   else //different start and end books
                   {
                       startingchapter = 0; //start at first chapter
                       chapterstocheck = tmpbook.chapters.Count; //total chapters in book
                   }
                   for (int y = startingchapter; y < chapterstocheck; y++)
                   {
                       versestoadd++;
                       tmpchapter = (Chapter)tmpbook.chapters[y];
                       if (endchapter.SelectedIndex == 0) //same start and end chapter
                       {
                           startingverse = startverse.SelectedIndex;
                           versestocheck = startingverse + endverse.SelectedIndex;
                       }
                       else
                       {
                           startingverse = 0;
                           versestocheck = tmpchapter.verse.Count;
                       }
                       for (int z = startingverse; z < versestocheck; z++)
                       {
                   
                       }
                   }
               }
               versestoadd++;
               theverse.Text = versestoadd.ToString() + " chapters within total selection range.";
               /*time.Text = tmp.text;
               theverse.Text = tmp.text;
               return;
           }
        */

        void showdisplayoptions_Click(object sender, EventArgs e)
        {
            //testing for DisplayStyleDialog
            DisplayStyleDialog displaydialog1 = new DisplayStyleDialog();
            IDisplayStyleTemplateManager templateManager = BibleClipProducer.TemplateManager;
            displaydialog1.TemplateManager = templateManager;
            displaydialog1.SelectedTemplate = templateManager.GetTemplate(DisplayStyle.Name);
            displaydialog1.DisplayStyle = DisplayStyle;
            DialogResult result = displaydialog1.ShowDialog();
            {
                if (result == DialogResult.OK)
                {
                    if (this.Live)
                    {
                        mainoutput.DisplayStyle = displaydialog1.DisplayStyle;
                        Invalidated(this, e);
                    }
                    DisplayStyle = displaydialog1.DisplayStyle as BibleDisplayStyle;
                    UpdateThumbnailImage();
                    if (displaydialog1.SelectedTemplate != null)
                    {
                        templateManager.MasterDisplayStyle = displaydialog1.SelectedTemplate;
                    }
                }
                displaydialog1.Dispose();
            }
            return;
        }

        void shownumberscheckbox_CheckedChanged(object sender, EventArgs e)
        {
            boolShowVerseNumbers = chkshownumbers.Checked;
            if (mainoutput != null) Invalidated(this, e);
        }

        void showtitlecheckbox_CheckedChanged(object sender, EventArgs e)
        {
            boolShowPassageTitle = chkshowtitle.Checked;
            if (mainoutput != null) Invalidated(this, e);
        }

        void BibleSearch_Click(object sender, EventArgs e)
        {
            int intZefaniaBookIndex;
            Resources.BibleSearchForm search = new Resources.BibleSearchForm();
            search.StrTranslationFile = StrTranslationFileName;
            DialogResult cancel = search.ShowDialog();
            if (cancel == DialogResult.Cancel)
            {
                search.Dispose();
                return;
            }
            if (!int.TryParse(search.StrBookName, out intZefaniaBookIndex))
            {
                for (int i = 0; i < cmbstartbook.Items.Count; i++)
                {
                    if (cmbstartbook.Items[i].ToString() == search.StrBookName) cmbstartbook.SelectedIndex = i;
                }
            }
            else cmbstartbook.SelectedIndex = intZefaniaBookIndex - 1;
            startbook_SelectionChangeCommitted(sender,e);
            cmbstartchapter.SelectedIndex = search.IntChapter - 1; //-1 to account for array index start at 0
            startchapter_SelectionChangeCommitted(sender, e);
            cmbstartverse.SelectedIndex = search.IntVerse - 1;
            startverse_SelectionChangeCommitted(sender, e);
            UpdateThumbnailImage();
            search.Dispose();
        }

        #endregion

        #region Utility Functions

        public String SetPassageTitle()
        {
            String strPassageTitle = bible.GetBookName(cmbstartbook.SelectedIndex);
            strPassageTitle += " ";
            strPassageTitle += (cmbstartchapter.SelectedIndex + 1).ToString();
            strPassageTitle += ":";
            strPassageTitle += (cmbstartverse.SelectedIndex + 1).ToString();
            return strPassageTitle;
        }

        public void UpdateThumbnailImage()
        {
            /*This function was taken from the Song Clip.  It may need to be reworked later*/
            Image imgX = (Image)Properties.Resources.Thumbnail;
            Graphics gThumbnail = Graphics.FromImage(imgX);

            StringFormat strfmtFormat = new StringFormat();
            strfmtFormat.Alignment = StringAlignment.Center;
            strfmtFormat.LineAlignment = StringAlignment.Center;

            Font fntCaption = new Font(DisplayStyle.MainText.Font.FontFamily, 12f,DisplayStyle.MainText.Font.Style);

            Color clrBackground = Color.FromArgb(190, DisplayStyle.BackgroundColour.R, DisplayStyle.BackgroundColour.G, DisplayStyle.BackgroundColour.B);
            Rectangle rectArea = new Rectangle(2, 0, imgX.Width - 4, imgX.Height);

            String strBiblename;
            if (bible.StrTranslationFilePath=="packaged"/*bible.BookOffset > 0*/)
            {
                //this indicates it's packaged, so get first book.
                strBiblename = bible.NewGetBookName(bible.IntBookOffset);
            }else strBiblename = bible.GetBookName(intSavedStartingBook);
            strBiblename = strBiblename + System.Environment.NewLine + (intSavedStartingChapter + 1).ToString() + ":" + (intSavedStartingVerse + 1).ToString();

            //Vertically alight the text area.
            rectArea.Height = Math.Min(rectArea.Height, (int)gThumbnail.MeasureString(strBiblename, fntCaption, rectArea.Width).Height);
            rectArea.Y = (imgX.Height - rectArea.Height) / 2;

            gThumbnail.FillRectangle(new SolidBrush(clrBackground), rectArea);
            
            gThumbnail.DrawString(strBiblename, fntCaption, new SolidBrush(DisplayStyle.MainText.ForeColour), rectArea, strfmtFormat);
            ThumbnailImage = imgX;
        }

        public String GetTranslationFileName(String strTranslationFilename)
        {

            DialogResult filechoice = new DialogResult();
            if (!File.Exists(strTranslationFilename)) //pick file
            {
                System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Title = "Please Select File for Bible Translation to Use:";
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.Filter = "Bible Translation (*.xmm,*.xml,*)|*.xmm;*.xml;*";
                openFileDialog1.AutoUpgradeEnabled = false; //Disables Vista/Win7 style file picker reverting to old style
                openFileDialog1.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                filechoice = openFileDialog1.ShowDialog();
                if (filechoice == DialogResult.Cancel) return null;
                strTranslationFilename = openFileDialog1.FileName;
            }
            if (!File.Exists(strTranslationFilename)) //if file still does not exist...
            {
                throw new FileNotFoundException("Could not find translation file: " + strTranslationFilename + ".  You will need to select a translation file.");
            }
            return strTranslationFilename;
        }

        public Panel SetupEditPanel() //using NewChapterCount, NewVerseCount, NewGetVerse.  
        {
                Panel pnlEditPanel = new Panel();
                pnlEditPanel.BackColor = System.Drawing.Color.White;
                /*This code is repeated for each button */

                lblversepreview = new System.Windows.Forms.Label();
                lblversepreview.Location = new System.Drawing.Point(0, 200);
                lblversepreview.Size = new Size(200, 200);
                //show loaded verse
                lblversepreview.Text = "";
                if (boolShowVerseNumbers) lblversepreview.Text += (intSavedStartingVerse + 1).ToString() + " ";
                lblversepreview.Text += bible.NewGetVerse(intSavedStartingBook, intSavedStartingChapter, intSavedStartingVerse);
                lblversepreview.Visible = true;
                pnlEditPanel.Controls.Add(lblversepreview);

                if (bible.StrTranslationFilePath == "packaged")
                {
                    //do not add other controls. It's packaged, so verse cant be changed now anyway.  Besides, the code to possibly do so is very buggy right now
                    return pnlEditPanel;
                }

                lblstartverse = new System.Windows.Forms.Label();
                lblstartverse.Location = new System.Drawing.Point(0, 25);
                lblstartverse.Size = new Size(350, 15);
                lblstartverse.Text = "Pick starting Verse (Must do First!):";
                /*    endverselabel = new Label();
                    endverselabel.Size = new Size(600, 15);
                    endverselabel.Location = new System.Drawing.Point(0, 100);
                    endverselabel.Text = "Pick Ending Verse (Currently just counts verses and shows how many were selected, when it will work...):";*/
                pnlEditPanel.Controls.Add(lblstartverse);
                //editpanel.Controls.Add(endverselabel);

                cmbstartbook = new System.Windows.Forms.ComboBox();
                cmbstartbook.Location = new System.Drawing.Point(0, 40);
                cmbstartbook.SelectionChangeCommitted += new EventHandler(startbook_SelectionChangeCommitted);
                cmbstartbook.LostFocus += new EventHandler(startbook_SelectionChangeCommitted);
                //startbook.LostFocus += new EventHandler(startbook_LostFocus);
                cmbstartbook.GotFocus += new EventHandler(startbook_GotFocus);
                //startbook.SelectedValueChanged += new EventHandler(startbook_SelectionChangeCommitted);
                /*endbook = new ComboBox();
                endbook.Location = new System.Drawing.Point(0, 120);
                endbook.SelectionChangeCommitted += new EventHandler(endbook_SelectionChangeCommitted);*/
                //foreach (Book i in bible.Books)
                for (int i = 0; i < bible.Books.Count; i++)
                {
                    cmbstartbook.Items.Add(bible.GetBookName(i));
                    //startbook.Items.Add(bible.NewGetBookName(i));
                    //endbook.Items.Add(i.name);
                }
                cmbstartbook.SelectedIndex = intSavedStartingBook -bible.IntBookOffset;
                cmbstartbook.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbstartbook.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbstartbook.TabIndex = 2;
                pnlEditPanel.Controls.Add(cmbstartbook);
                //editpanel.Controls.Add(endbook);

                cmbstartchapter = new System.Windows.Forms.ComboBox();
                cmbstartchapter.Location = new System.Drawing.Point(130, 40);
                cmbstartchapter.Size = new System.Drawing.Size(50, 25);
                cmbstartchapter.TabIndex = 3;
                cmbstartchapter.SelectionChangeCommitted += new EventHandler(startchapter_SelectionChangeCommitted);
                //startchapter.GotFocus +=new EventHandler(startbook_SelectionChangeCommitted);
                cmbstartchapter.LostFocus += new EventHandler(startchapter_SelectionChangeCommitted);
                cmbstartverse = new System.Windows.Forms.ComboBox();
                for (int i = 0; i < bible.NewChapterCount(cmbstartbook.SelectedIndex)/*tmp1.chapters.Count*/; i++)
                {
                    int chapternum = i + 1;
                    cmbstartchapter.Items.Add(chapternum.ToString());
                }
                cmbstartchapter.SelectedIndex = intSavedStartingChapter -bible.IntChapterOffset;
                cmbstartchapter.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbstartchapter.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbstartverse.Location = new System.Drawing.Point(200, 40);
                cmbstartverse.Size = new System.Drawing.Size(50, 25);
                cmbstartverse.TabIndex = 4;
                cmbstartverse.SelectionChangeCommitted += new EventHandler(startverse_SelectionChangeCommitted);
                //startverse.GotFocus += new EventHandler(startchapter_SelectionChangeCommitted);
                cmbstartverse.LostFocus += new EventHandler(startverse_SelectionChangeCommitted);
                for (int i = 0; i < bible.NewVerseCount(cmbstartbook.SelectedIndex, cmbstartchapter.SelectedIndex); i++)
                {
                    int versenumber = i + 1;
                    cmbstartverse.Items.Add(versenumber.ToString());
                }
                cmbstartverse.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbstartverse.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbstartverse.SelectedIndex = intSavedStartingVerse -bible.IntVerseOffset;
                pnlEditPanel.Controls.Add(cmbstartchapter);
                pnlEditPanel.Controls.Add(cmbstartverse);

                chkshownumbers = new CheckBox();
                chkshownumbers.Text = "Show Verse Number(s)";
                chkshownumbers.Size = new System.Drawing.Size(150, 25);
                chkshownumbers.Location = new System.Drawing.Point(300, 120);
                chkshownumbers.Checked = boolShowVerseNumbers;
                chkshownumbers.CheckedChanged += new EventHandler(shownumberscheckbox_CheckedChanged);
                pnlEditPanel.Controls.Add(chkshownumbers);

                chkshowtitle = new CheckBox();
                chkshowtitle.Text = "Show Passage Title";
                chkshowtitle.Size = new System.Drawing.Size(150, 25);
                chkshowtitle.Location = new System.Drawing.Point(300, 150);
                chkshowtitle.Checked = boolShowPassageTitle;
                chkshowtitle.CheckedChanged += new EventHandler(showtitlecheckbox_CheckedChanged);
                pnlEditPanel.Controls.Add(chkshowtitle);

                btnshowdisplayoptions = new Button();
                btnshowdisplayoptions.Text = "Display Options";
                btnshowdisplayoptions.Size = new System.Drawing.Size(150, 25);
                btnshowdisplayoptions.Location = new System.Drawing.Point(300, 80);
                btnshowdisplayoptions.Click += new EventHandler(showdisplayoptions_Click);
                pnlEditPanel.Controls.Add(btnshowdisplayoptions);

                /*           endchapter = new ComboBox();
                           endchapter.Location = new System.Drawing.Point(130, 120);
                           endchapter.Size = new System.Drawing.Size(50, 25);
                           endchapter.SelectionChangeCommitted += new EventHandler(endchapter_SelectionChangeCommitted);
                           endverse = new ComboBox();
                           endverse.Location = new System.Drawing.Point(200, 120);
                           endverse.Size = new System.Drawing.Size(50, 25);
                           endverse.SelectionChangeCommitted += new EventHandler(endverse_SelectionChangeCommitted);
                           editpanel.Controls.Add(endchapter);
                           editpanel.Controls.Add(endverse);
                */
                
                   btnnext = new Button();
                   btnnext.Location = new System.Drawing.Point(200, 0);
                   btnnext.Size = new Size(150, 25);
                   btnnext.Text = "Next Verse";
                   btnnext.Click += new EventHandler(this.Next_Click);
                   btnnext.TabIndex = 1;
                   pnlEditPanel.Controls.Add(btnnext);

                   btnprevious = new Button();
                   btnprevious.Location = new System.Drawing.Point(0, 0);
                   btnprevious.Size = new Size(150,25);
                   btnprevious.Text = "Previous Verse";
                   btnprevious.Click += new EventHandler(this.Prev_Click);
                   btnprevious.TabIndex = 0;
                   pnlEditPanel.Controls.Add(btnprevious);

                   btnsearch = new Button();
                   btnsearch.Location = new System.Drawing.Point(400, 0);
                   btnsearch.Size = new Size(150, 25);
                   btnsearch.Text = "Search Bible";
                   btnsearch.Click += new EventHandler(this.BibleSearch_Click);
                   btnsearch.TabIndex = 15;
                   pnlEditPanel.Controls.Add(btnsearch);
                return pnlEditPanel;
        }
        
        #endregion

        #region ICueClip

        public bool CueingEnabled  //always allow cueing, so setting to always return true
        {
            get 
            {
                return true;
            }
            set { return;  }
        }

        public Form CueControl
        {
            get {
                Resources.CueClipForm frmCueClip = new Resources.CueClipForm();
                Panel pnlCue;
                if (this.Live)
                {
                    pnlCue = new Panel();
                    Label lblMessage = new Label();
                    lblMessage.Text = "The Cue/Edit Window is disabled because the clip is playing.  Please close this window and use the Live Monitor instead to change verses.  Fade appropriate layer down if needed to change verse without screen display first.";
                    lblMessage.Width = 400;
                    lblMessage.Height = 400;
                    pnlCue.Controls.Add(lblMessage);
                    //return null;
                }
                else pnlCue=SetupEditPanel();
                frmCueClip.FormClosing += new FormClosingEventHandler(frmcueclip_formclosing);
                pnlCue.Width = frmCueClip.Width;
                pnlCue.Height = frmCueClip.Height;
                frmCueClip.Controls.Add(pnlCue);
                return frmCueClip; 
            }
        }

        private void frmcueclip_formclosing(object sender, FormClosingEventArgs e)
        {
            if (this.Live)
            {
                int intBook,intChap,intVerse;
                intBook = cmbstartbook.SelectedIndex;
                intChap = cmbstartchapter.SelectedIndex;
                intVerse = cmbstartverse.SelectedIndex;
                ReleaseLiveMonitor(livemonitor);
                livemonitor = CreateLiveMonitor() as Panel;
                //livemonitor = null;
                //livemonitor = SetupEditPanel();
                cmbstartbook.SelectedIndex = intBook;
                cmbstartchapter.SelectedIndex = intChap;
                cmbstartverse.SelectedIndex = intVerse;
            }
            UpdateThumbnailImage();
            return;
        }

        #endregion

        #region Clip Operations
        protected void SetupLiveMonitor()
        {
            livemonitor = SetupEditPanel();

        }

        protected override void OnInitialiseShow(System.Type RenderInterface, System.Type x) //Needed for 3.6
        //protected override void OnInitialiseShow(System.Type RenderInterface) //Needed for 3.5
        {
            try
            {
                buildmainoutput();
                setupdisplaystyle();

                SetupLiveMonitor();

                mainoutput.MainWords = lblversepreview.Text;
                mainoutput.SectionIndex = 0;

                if (boolShowPassageTitle)
                {
                    mainoutput.SetSectionText("Passage Title", SetPassageTitle()); //Set Title of Passage
                }
                else
                {
                    mainoutput.SetSectionText("Passage Title", "");
                }

                mainoutput.SetSectionText("Copyright", bible.StrCopyrightInfo);

                livemonitor.Invalidate(); /*To Repaint the Live Monitor Area*/
                Invalidated(this, new EventArgs());
            }
            catch (System.Exception e)
            {
                OnUnhandledException(e);
                return;
            }
        }

        public override void StopShow()
        {
            //close BibleWindow
            mainoutput = null;
            livemonitor = null;
        }

        #endregion

        #region Properties & Resources
        [Obsolete]
        public override System.Windows.Forms.Control LiveMonitor
        {
            get
            {
                return livemonitor;
            }
        }

        public override System.Windows.Forms.Control MainOutput
        {
            get
            {
                return mainoutput;
            }
        }

        public override string TypeName
        {
            get
            {
                return "Bible Clip";
            }
        }

        public override string Caption
        {
            get
            {
                return "Bible Clip";
            }
        }

        public override string Description
        {
            get { return "Loads a xml bible translation file and displays it."; }
        }

        List<ToolStripMenuItem> IMenuSettings.MenuItems
        {
            get
            {
                List<ToolStripMenuItem> lstItems = new List<ToolStripMenuItem>();
                lstItems.Add(loadnewtranslationfile);
                lstItems.Add(showdisplayoptions);
                return lstItems;
            }
        }
        #endregion
    }


}