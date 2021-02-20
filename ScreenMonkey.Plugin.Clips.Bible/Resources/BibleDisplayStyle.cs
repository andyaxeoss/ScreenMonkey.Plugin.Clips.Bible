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
using ScreenMonkey.Text;
using ScreenMonkey.Text.Formatter;
using Oarw.General;

using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ScreenMonkey.Plugin.Clips.Bible
{
    public class BibleDisplayStyle: TextDisplayStyleBase, IXmlPersistWithPathRewrite
    {
        public BibleDisplayStyle()
        {
            Initialize();
            return;
        }

        public BibleDisplayStyle(BibleDisplayStyle source)
        {
            Initialize();
            Copy(source);
            return;
        }

        void Initialize()
        {
            AddTextItem(new StaticTextFormatter("Passage Title"));
            AddTextItem(new StaticTextFormatter("Main Text"));
            AddTextItem(new StaticTextFormatter("Copyright"));
            MainText.Margin = new PaddingF(0.03f, 0.10f, 0.03f, 0.10f);
            MainText.TransitionEnabled = true;
            MainText.Font = new Font(FontFamily.GenericSansSerif, 28);
            MainText.ForeColour = Color.White;
            CopyRight.Margin = new PaddingF(0.01f,0.9f,0.01f,0.01f);
            CopyRight.Alignment = ContentAlignment.BottomLeft;
            CopyRight.Font = new Font(FontFamily.GenericSansSerif, 12);
            CopyRight.ForeColour = Color.White;
            PassageTitle.Margin = new PaddingF(0.01f, 0.01f, 0.01f, 0.9f);
            PassageTitle.Alignment = ContentAlignment.TopCenter;
            PassageTitle.Font = new Font(FontFamily.GenericSansSerif, 8);
            PassageTitle.ForeColour = Color.White;
            strBackgroundImage = "";
            backgroundcolor = Color.Black;
            boolBackgroundTransparent = true;
            PassageTitle.Changed += new EventHandler(TextItem_Changed);
            MainText.Changed +=new EventHandler(TextItem_Changed);
            CopyRight.Changed += new EventHandler(TextItem_Changed);
            return;
        }

        void TextItem_Changed(object sender, EventArgs e)
        {
            OnChanged();
        }

        private int intTransitionTime = 250;
        private Color backgroundcolor;
        private string strBackgroundImage;
        bool boolBackgroundTransparent;

        public override int TransitionTime
        {
            get
            {
                return intTransitionTime;
            }
            set
            {
                intTransitionTime = value;
            }
        }

        public override Color BackgroundColour
        {
            get
            {
                return backgroundcolor;
            }
            set
            {
                backgroundcolor = value;
                OnChanged();
            }
        }

        public override string BackgroundImage
        {
            get
            {
                return strBackgroundImage;
            }
            set
            {
                strBackgroundImage = value;
                OnChanged();
            }
        }

        public override Bitmap BackgroundImageCached
        {
            get 
            {
                //int width = ; //how to set proper width?
                //int height; //how to set proper height?
                //return new Bitmap(BackgroundImage,width,height);
                //return null;
                return new Bitmap(BackgroundImage);
            }
        }

        public override bool BackgroundTransparent
        {
            get
            {
                return boolBackgroundTransparent;
            }
            set
            {
                boolBackgroundTransparent = value;
                OnChanged();
            }
        }

        private StaticTextFormatter mainText = new StaticTextFormatter("Main Text");
        public StaticTextFormatter MainText
        {
            get
            {
                return (StaticTextFormatter) TextItem("Main Text");
            }
        }

        private StaticTextFormatter copyRight = new StaticTextFormatter("Copyright");
        public StaticTextFormatter CopyRight
        {
            get
            {
                return (StaticTextFormatter) TextItem("Copyright");
            }
        }

        private StaticTextFormatter passageTitle = new StaticTextFormatter("Passage Title");
        public StaticTextFormatter PassageTitle
        {
            get
            {
                return (StaticTextFormatter) TextItem("Passage Title");
            }
        }

        public override void Copy(ITextDisplayStyle source)
        {
            Name = source.Name;
            BackgroundTransparent = source.BackgroundTransparent;
            BackgroundColour = source.BackgroundColour;
            BackgroundImage = source.BackgroundImage;
            TransitionTime = source.TransitionTime;
            BibleDisplayStyle bibleSource = source as BibleDisplayStyle;
            if (bibleSource != null)
            {
                PassageTitle.Copy(bibleSource.PassageTitle);
                CopyRight.Copy(bibleSource.CopyRight);
                MainText.Copy(bibleSource.MainText);
            }
            OnChanged();
        }

        public void SaveXml(System.Xml.XmlWriter Writer, ConformPathHandler pathRewrite)
        {
            Writer.WriteStartElement("style");
            Writer.WriteAttributeString("name", Name);
            Writer.WriteAttributeString("transparent", BackgroundTransparent.ToString());
            Writer.WriteAttributeString("backColour", BackgroundColour.ToArgb().ToString());
            Writer.WriteAttributeString("backImage", BackgroundImage);
            Writer.WriteAttributeString("transitionTime", TransitionTime.ToString());

            SaveTextItem(Writer, PassageTitle, "passageTitle");
            SaveTextItem(Writer, MainText, "mainText");
            SaveTextItem(Writer, CopyRight, "copyrightText");

            Writer.WriteEndElement();
        }

        public void SaveTextItem(System.Xml.XmlWriter Writer,StaticTextFormatter item, string name)
        {
            Writer.WriteStartElement(name);
            Writer.WriteAttributeString("colour", item.ForeColour.ToArgb().ToString());
            Writer.WriteAttributeString("fontFamily", item.Font.FontFamily.Name);
            Writer.WriteAttributeString("fontSize", item.Font.Size.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Writer.WriteAttributeString("fontStyle", item.Font.Style.ToString());
            Writer.WriteAttributeString("alignment", item.Alignment.ToString());
            Writer.WriteAttributeString("margin", item.Margin.ToString());

            Writer.WriteStartElement("textOutline");
            Writer.WriteAttributeString("opacity", item.TextOutline.Opacity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Writer.WriteAttributeString("colour", item.TextOutline.Color.ToArgb().ToString());
            Writer.WriteAttributeString("thickness", item.TextOutline.Thickness.ToString());
            Writer.WriteEndElement();

            Writer.WriteStartElement("dropShadow");
            Writer.WriteAttributeString("opacity", item.DropShadow.Opacity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Writer.WriteAttributeString("colour", item.DropShadow.Color.ToArgb().ToString());
            Writer.WriteAttributeString("angle", item.DropShadow.Direction.ToString());
            Writer.WriteAttributeString("altitude", item.DropShadow.Altitude.ToString());
            Writer.WriteEndElement();

            Writer.WriteEndElement();


        }

        public void LoadXml(System.Xml.XmlNode Settings)
        {
            try
            {
                System.Xml.XmlNode nameXml = Settings.SelectSingleNode("style/@name");
                System.Xml.XmlNode transparentNode = Settings.SelectSingleNode("style/@transparent");
                System.Xml.XmlNode backColourNode = Settings.SelectSingleNode("style/@backColour");
                System.Xml.XmlNode backImageNode = Settings.SelectSingleNode("style/@backImage");
                System.Xml.XmlNode transitionTimeNode = Settings.SelectSingleNode("style/@transitionTime");

                if (nameXml != null) Name = nameXml.Value;
                if (transparentNode != null) BackgroundTransparent = bool.Parse(transparentNode.Value);
                if (backColourNode != null) BackgroundColour = Color.FromArgb(int.Parse(backColourNode.Value));
                if (backImageNode != null) BackgroundImage = backImageNode.Value;
                if (transitionTimeNode != null) TransitionTime = int.Parse(transitionTimeNode.Value);

                loaditemtext(Settings.SelectSingleNode("style/passageTitle"), PassageTitle);
                loaditemtext(Settings.SelectSingleNode("style/mainText"), MainText);
                loaditemtext(Settings.SelectSingleNode("style/copyrightText"), CopyRight);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void loaditemtext(System.Xml.XmlNode itemNode, StaticTextFormatter textItem)
        {
            if (itemNode != null)
            {
                //Load the standard styles

                System.Xml.XmlNode textColourNode = itemNode.SelectSingleNode("@colour");
                System.Xml.XmlNode textFontNode = itemNode.SelectSingleNode("@fontFamily");
                System.Xml.XmlNode textFontSizeNode = itemNode.SelectSingleNode("@fontSize");
                System.Xml.XmlNode textFontStyleNode = itemNode.SelectSingleNode("@fontStyle");
                System.Xml.XmlNode alignmentNode = itemNode.SelectSingleNode("@alignment");
                System.Xml.XmlNode marginNode = itemNode.SelectSingleNode("@margin");

                if (textColourNode != null) textItem.ForeColour = Color.FromArgb(int.Parse(textColourNode.Value));
                if (textFontNode != null)
                {
                    FontStyle style = FontStyle.Regular;
                    if (textFontStyleNode != null)
                    {
                        style = (FontStyle)Enum.Parse(typeof(FontStyle), textFontStyleNode.Value);

                    }
                    textItem.Font = new Font(textFontNode.Value, float.Parse(textFontSizeNode.Value, System.Globalization.CultureInfo.InvariantCulture), style);
                }
                if (alignmentNode != null) textItem.Alignment = (ContentAlignment)Enum.Parse(typeof(ContentAlignment), alignmentNode.Value);
                if (marginNode != null) textItem.Margin = PaddingF.Parse(marginNode.Value);

                //Load Text Outline Style
                System.Xml.XmlNode outlineOpacityNode = itemNode.SelectSingleNode("textOutline/@opacity");
                System.Xml.XmlNode outlineColourNode = itemNode.SelectSingleNode("textOutline/@colour");
                System.Xml.XmlNode outlineThicknessNode = itemNode.SelectSingleNode("textOutline/@thickness");

                if (outlineOpacityNode != null) textItem.TextOutline.Opacity = float.Parse(outlineOpacityNode.Value, System.Globalization.CultureInfo.InvariantCulture);
                if (outlineColourNode != null) textItem.TextOutline.Color = Color.FromArgb(int.Parse(outlineColourNode.Value));
                if (outlineThicknessNode != null) textItem.TextOutline.Thickness = int.Parse(outlineThicknessNode.Value);

                //Load Drop Shadow Style
                System.Xml.XmlNode shadowOpacityNode = itemNode.SelectSingleNode("dropShadow/@opacity");
                System.Xml.XmlNode shadowColourNode = itemNode.SelectSingleNode("dropShadow/@colour");
                System.Xml.XmlNode shadowAngleNode = itemNode.SelectSingleNode("dropShadow/@angle");
                System.Xml.XmlNode shadowAltitudeNode = itemNode.SelectSingleNode("dropShadow/@altitude");

                if (shadowOpacityNode != null) textItem.DropShadow.Opacity = float.Parse(shadowOpacityNode.Value, System.Globalization.CultureInfo.InvariantCulture);
                if (shadowColourNode != null) textItem.DropShadow.Color = Color.FromArgb(int.Parse(shadowColourNode.Value));
                if (shadowAngleNode != null) textItem.DropShadow.Direction = int.Parse(shadowAngleNode.Value);
                if (shadowAltitudeNode != null) textItem.DropShadow.Altitude = int.Parse(shadowAltitudeNode.Value);
            }
        }

        public override object Clone()
        {
            return new BibleDisplayStyle(this);
        }
    }
}