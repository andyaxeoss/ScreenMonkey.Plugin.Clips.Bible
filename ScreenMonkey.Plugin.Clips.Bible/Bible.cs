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
using System.IO;
using System.Xml;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

namespace ScreenMonkey.Plugin.Clips.Bible
{
    class Bible
    {
        public XmlReader XmlTranslation;
        public XmlDocument XmlCurrentBook; //temporarily used to hold book xml for functions
        public XmlNode XmlPackagedBible;
        public int IntSelectedBook=-1;
        public int IntSelectedChapter=-1;
        public int IntSelectedVerse = -1;
        public int IntBookOffset = 0;
        public int IntChapterOffset = 0;
        public int IntVerseOffset = 0;
        public String StrTranslationFilePath;
        public String StrCopyrightInfo;
        public bool BoolTranslationChanged = false;
        public ArrayList Books = new ArrayList(); //when GetBookName is replaced fully by NewGetBookName, will be phased out

        #region structures

        public struct Book  //when GetBookName is replaced fully by NewGetBookName, will be phased out
        {
            public int intIndex;
            public String strName;
        }
        #endregion

        #region functions

        public static bool IsBibleXmlFile(System.IO.FileInfo file)
        {
            XmlDocument xmlTestFile = new XmlDocument();
            xmlTestFile.Load(file.FullName);
            if(xmlTestFile.SelectSingleNode("/XMLBIBLE/BIBLEBOOK/CHAPTER/VERS") is XmlNode)
            {
                return true;
            }
            if (xmlTestFile.SelectSingleNode("/bible/b/c/v") is XmlNode)
            {
                return true;
            }
            return false;
        }

        public void LoadPackagedBook(int intBook)
        {           
            XmlCurrentBook = new XmlDocument();
            XmlCurrentBook.LoadXml(XmlPackagedBible.SelectSingleNode("BIBLEBOOK").OuterXml);
            IntSelectedBook = intBook;
        }

        public void LoadBook(int intBook)
        {
            if (IntSelectedBook == intBook && !BoolTranslationChanged) return; //correct book is already loaded
            if (StrTranslationFilePath == "packaged")
            {
                LoadPackagedBook(intBook);
                return;
            }
            using (XmlTranslation = new XmlTextReader(StrTranslationFilePath))
            {
                BoolTranslationChanged = false; //reset counter because it's being changed right now
                int intBookRead = -1;
                while (intBookRead < intBook && !XmlTranslation.EOF)
                {
                    XmlTranslation.Read();
                    if (XmlTranslation.NodeType == XmlNodeType.Element && (XmlTranslation.Name == "BIBLEBOOK"))
                    {
                        intBookRead = Convert.ToInt32(XmlTranslation.GetAttribute("bnumber"));
                        intBookRead --; //have to subtract one because book names are indexed starting at 0, but zefania starts counting from 1
                    }
                    if (XmlTranslation.NodeType == XmlNodeType.Element && (XmlTranslation.Name == "b"))
                    {
                        intBookRead++;
                    }
                }
                if (XmlTranslation.EOF)
                {
                    XmlCurrentBook = null;
                    XmlTranslation.Close();
                    throw new FileNotFoundException("Could not load desired book in function LoadBook - translation file may not be properly read");
                }
                XmlCurrentBook = new XmlDocument();
                XmlCurrentBook.LoadXml(XmlTranslation.ReadOuterXml());
                IntSelectedBook = intBook;
            }
            XmlTranslation.Close();
        }
       
        public String NewGetBookName(int intBook) //Works, but takes too long when playing clip.  Using old GetBookName instead.
        {
            LoadBook(intBook);
            if (XmlCurrentBook != null)
            {
                XmlNode BookXmlNode = XmlCurrentBook.SelectSingleNode("/BIBLEBOOK/@bname");
                if (BookXmlNode == null) XmlCurrentBook.SelectSingleNode("/b/@n");
                return BookXmlNode.Value.ToString();
            }
            return null;
        }

        public String GetBookName(int book) //current implementation, to be possibly replaced by NewGetBookName
        {
            Book tmp1 = new Book();
            for(int i=0;i<Books.Count;i++)
            {
                tmp1= (Book)Books[book];
                if (tmp1.intIndex == book) break;
            }
            return tmp1.strName;
        }

        public int NewChapterCount(int intBook)
        {
            if (intBook < 0) intBook = 0;
            LoadBook(intBook);
            if (XmlCurrentBook != null)
            {
                XmlNodeList xmlChapterXmlNodes = XmlCurrentBook.DocumentElement.SelectNodes("/BIBLEBOOK/CHAPTER");
                if (xmlChapterXmlNodes.Count ==0) xmlChapterXmlNodes = XmlCurrentBook.DocumentElement.SelectNodes("/b/c");
                if (xmlChapterXmlNodes.Count == 0) throw new Exception("Unable to get number of chapters - Bible.NewChapterCount()");
                return xmlChapterXmlNodes.Count;
            }
            return 0;
        }
        public int NewVerseCount(int intBook, int intChapter) //Works and is being used
        {
            LoadBook(intBook);
            if (XmlCurrentBook != null)
            {
                String xPath="/BIBLEBOOK/CHAPTER[@cnumber="+(intChapter+1).ToString()+"]/VERS";
                XmlNodeList verseXmlNode = XmlCurrentBook.DocumentElement.SelectNodes(xPath); //Zefania format
                if (verseXmlNode.Count==0) //could not find in Zefania format, must be Opensong format
                {
                    xPath = "/b/c[" + (intChapter+1).ToString() + "]/v";
                    verseXmlNode = XmlCurrentBook.SelectNodes(xPath); //OpenSong format
                }
                if (verseXmlNode.Count == 0) throw new Exception("No XML Nodes were selected with current XPath - function NewVerseCount");
                return verseXmlNode.Count;
            }
            else throw new Exception("Not able to load book - function NewVerseCount");
        }

        public String NewGetVerse(int intBook, int intChapter, int intVerse)
        {
            LoadBook(intBook);
            if (XmlCurrentBook != null)
            {
                XmlNode verseXmlNode = XmlCurrentBook.SelectSingleNode("/BIBLEBOOK/CHAPTER[@cnumber="+(intChapter+1).ToString()+"]/VERS[@vnumber=" + (intVerse+1).ToString() + "]/text()"); //Zefania format
                if (verseXmlNode == null) verseXmlNode = XmlCurrentBook.SelectSingleNode("/b/c["+(intChapter+1).ToString()+"]/v[" + (intVerse+1).ToString() + "]/text()"); //OpenSong format
                IntSelectedVerse = intVerse;
                return verseXmlNode.Value;
            }
            return null;
        }

        public void LoadTranslationFile(string strFilename)
        {
            Books.Clear();
            Book tmpbook = new Book();
            if (strFilename.Contains("packaged"))
            {
                StrTranslationFilePath = "packaged";
                return; //If Packaged, then Bible.Load(XmlNode) does it from OnLoadSettings
            }
            try
            {
                if (!File.Exists(strFilename)) throw new FileNotFoundException("Could not find translation file - function name - LoadTranslationFile");
                StrTranslationFilePath = strFilename;
                StrCopyrightInfo = "";
                using (XmlTranslation = new XmlTextReader(strFilename))
                {
                    int b = 0; //books
                    while (XmlTranslation.Read())
                    {
                        switch (XmlTranslation.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (XmlTranslation.Name)
                                {
                                    case "BIBLEBOOK": //add new book (Zelfania Format)
                                        b++; //index of book;
                                        tmpbook = new Book();
                                        tmpbook.intIndex = Convert.ToInt32(XmlTranslation.GetAttribute("bnumber"));
                                        tmpbook.strName = XmlTranslation.GetAttribute("bname");
                                        break;
                                    case "b": //add new book
                                        b++; //index of book;
                                        tmpbook = new Book();
                                        tmpbook.intIndex = b;
                                        tmpbook.strName = XmlTranslation.GetAttribute("n");
                                        break;
                                    //below 3 tags are subtags of Information Tag
                                    case "title":
                                        StrCopyrightInfo += " "+XmlTranslation.ReadString(); //Add Title to Copyright Info
                                        break;
                                    case "publisher":
                                        StrCopyrightInfo += " Published by: " + XmlTranslation.ReadString(); //Add Publisher Info
                                        break;
                                    case "rights":
                                        StrCopyrightInfo += " " + XmlTranslation.ReadString(); //add copyright information
                                        break;
                                }
                                break;
                            case XmlNodeType.EndElement:
                                switch (XmlTranslation.Name)
                                {
                                    case "BIBLEBOOK":
                                    case "b": //end of book                             
                                        Books.Add(tmpbook);
                                        break;
                                }
                                break;
                        }
                    }
                    XmlTranslation.Close();
                }
            }
            catch (FileNotFoundException e)
            {
                Books.Clear();
                throw e;
            }
            catch (Exception e)
            {
                Books.Clear();
                XmlTranslation.Close();
                throw e;
            }

        }

        public void Load(XmlNode xmlBible)
        {
            XmlPackagedBible = xmlBible.SelectSingleNode("XMLBIBLE");
        }

        public void SaveXML(XmlWriter xmlWriter)
        {
            //this function gets selected verses from bible and saves them to specified file.
            if (IntSelectedBook < 0) IntSelectedBook = 0;
            if (IntSelectedChapter < 0) IntSelectedChapter = 1;
            if (IntSelectedVerse <= 0) IntSelectedVerse = 1;

            Book x = (Book) Books[IntSelectedBook];

            xmlWriter.WriteStartElement("XMLBIBLE");

            xmlWriter.WriteStartElement("INFORMATION");
            xmlWriter.WriteStartElement("title");
            xmlWriter.WriteString(StrCopyrightInfo);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("BIBLEBOOK");
            xmlWriter.WriteAttributeString("bname", x.strName.ToString());
            xmlWriter.WriteAttributeString("bnumber", x.intIndex.ToString());
            xmlWriter.WriteStartElement("CHAPTER");
            xmlWriter.WriteAttributeString("cnumber", (IntSelectedChapter).ToString());
            xmlWriter.WriteStartElement("VERS");
            xmlWriter.WriteAttributeString("vnumber", (IntSelectedVerse).ToString());
            xmlWriter.WriteString(NewGetVerse(IntSelectedBook,IntSelectedChapter-1,IntSelectedVerse-1));
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
        }

        #endregion
    }
}