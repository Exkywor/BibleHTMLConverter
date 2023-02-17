using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BibleHTMLConverter {
    public static class BibleWriter {
        private static XmlDocument _document;
        private static XmlTextWriter _writer;

        private static XmlElement _bibleEl;
        /// <summary>
        /// Creates a new bible .xml file and prepare it for writing.
        /// </summary>
        /// <param name="outputPath">Bible file path.</param>
        public static void StartBibleWriter(string outputPath) {
            _document = new();
            _bibleEl = _document.CreateElement("bible");
            _writer = new(outputPath, null);
            _writer.Formatting = Formatting.Indented;
        }

        /// <summary>
        /// Add a book to the bible XML document.
        /// </summary>
        /// <param name="book">Book to add.</param>
        public static void AddBook(Book book)
        {
            XmlElement bookEl = _document.CreateElement("b");
            bookEl.SetAttribute("n", book.GetBookName());

            foreach (int chapterNumber in book.GetChapterList())
            {
                Chapter? chapter = book.GetChapter(chapterNumber);
                if (chapter != null)
                {
                    AddChapter(bookEl, chapterNumber, chapter);
                }
            }

            _bibleEl.AppendChild(bookEl);
        }

        /// <summary>
        /// Add a chapter to the book element.
        /// </summary>
        /// <param name="bookEl">Book element to append chapters to.</param>
        /// <param name="chapterNumber">Chapter number.</param>
        /// <param name="chapter">Chapter.</param>
        private static void AddChapter(XmlElement bookEl, int chapterNumber, Chapter chapter)
        {
            XmlElement chapterEl = _document.CreateElement("c");
            chapterEl.SetAttribute("n", chapterNumber.ToString());

            foreach (int verseNumber in chapter.GetVerseList())
            {
                AddVerse(chapterEl, verseNumber, chapter.GetVerse(verseNumber));
            }

            bookEl.AppendChild(chapterEl);
        }

        /// <summary>
        /// Add a verse to the chapter element.
        /// </summary>
        /// <param name="chapterEl">Chapter element to append verse to.</param>
        /// <param name="verseNumber">Verse number.</param>
        /// <param name="verse">Verse.</param>
        private static void AddVerse(XmlElement chapterEl, int verseNumber, string verse)
        {
            XmlElement verseEl = _document.CreateElement("v");
            verseEl.SetAttribute("n", verseNumber.ToString());
            verseEl.InnerText = verse;
            chapterEl.AppendChild(verseEl);
        }

        /// <summary>
        /// Overwrite the document with the bible element.
        /// </summary>
        public static void UpdateBible()
        {
            _document.RemoveAll();
            _document.AppendChild(_bibleEl);
            _document.Save(_writer);
        }
    }
}
