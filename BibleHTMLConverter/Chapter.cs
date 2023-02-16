using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleHTMLConverter {
    /// <summary>
    /// Represents a Chapter in a Book.
    /// Verse numbers is stored as keys to account for possible errors in the source files.
    /// </summary>
    public class Chapter {
        private int _chapter;
        private Dictionary<int, string> _verses;

        public Chapter(int chapter)
        {
            _chapter = chapter;
            _verses = new();
        }

        public int GetChapter() => _chapter;
        public void SetChapter(int chapter) => _chapter = chapter;

        public string GetVerse(int verseNum)
        {
            if (_verses.ContainsKey(verseNum))
            {
                return _verses[verseNum];
            } else
            {
                return ("Verse not found.");
            }
        }
        public void SetVerse(int verseNum, string verseText)
        {
            _verses[verseNum] = verseText;
        }
    }
}
