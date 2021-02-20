#region Copyright © 2011 Oliver Waits, Andy Axe, online community
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

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using ScreenMonkey.Clips;
using ScreenMonkey.Clips.Extensions;
using ScreenMonkey.Text;

namespace ScreenMonkey.Plugin.Clips.Bible
{

    public class BibleClipProducer : ClipMediaProducerBase
    {

        public BibleClip NewClip;

        #region Information
        public override string Name
        {
            get { return "Bible Clip"; }
        }

        public override string Caption
        {
            get { return "Bible Clip"; }
        }

        public override string Description
        {
            get { return "Loads a Zefania or OpenSong formatted Bible file to present in Screen Monkey."; }
        }
     
        public override Type ClipMediaType
        {
            get { return typeof(BibleClip); }
        }

        public override System.Drawing.Image Logo
        {
            get
            {
                Image x = (Image)ScreenMonkey.Plugin.Clips.Bible.Properties.Resources.Logo;
                return x;
            }
        }

        #endregion

        #region Creation
        public override ClipMediaBase Create()
        {
            NewClip = new BibleClip();
            NewClip.StrTranslationFileName = NewClip.GetTranslationFileName(null);
            if (NewClip.StrTranslationFileName == null) return null;
            NewClip.DisplayStyle = (BibleDisplayStyle) TemplateManager.MasterDisplayStyle;
            return NewClip;
        }

        public override ClipMediaBase Create(System.IO.FileInfo file)
        {
            NewClip = new BibleClip();
            NewClip.StrTranslationFileName = NewClip.GetTranslationFileName(file.FullName);
            if (NewClip.StrTranslationFileName == null) return null;
            NewClip.DisplayStyle = (BibleDisplayStyle)TemplateManager.MasterDisplayStyle;
            return NewClip;
        }

        public override bool IsFileSupported(System.IO.FileInfo file)
        {
            if (file.Extension == ".xml" || file.Extension == "" || file.Extension == ".xmm")
            {
                return Bible.IsBibleXmlFile(file);
            }

            return false;
        }

        

        #endregion

        #region Saving/Loading

        protected override void OnSaveSettings(XmlWriter xmlSettings)
        {
            //TODO Determine if 2nd parameter needs initialized, and if so to what?
            templateManager.SaveXml(xmlSettings, null);
        }

        protected override void OnLoadSettings(XmlNode xmlSettings)
        {
            templateManager.LoadXml(xmlSettings);
            return;
        }

        #endregion

        #region DisplayStyleTemplate

        private static DisplayStyleTemplateManager<BibleDisplayStyle> templateManager = 
            new DisplayStyleTemplateManager<BibleDisplayStyle>();

        public static IDisplayStyleTemplateManager TemplateManager
        {
            get { return templateManager; }
        }

        #endregion
    }
}