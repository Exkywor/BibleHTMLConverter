using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleHTMLConverter {
    /// <summary>
    /// Represents a Chapter in a Book.
    /// Verse numbers are stored as keys to account for possible errors in the source files.
    /// </summary>
    public class Chapter {
        private Dictionary<int, string> _verses = new();

        /// <summary>
        /// Get the input verse.
        /// </summary>
        /// <param name="verseNum">Verse to find.</param>
        /// <returns>The verse text, if it exists.</returns>
        public string GetVerse(int verseNum)
        {
            if (_verses.ContainsKey(verseNum))
            {
                return _verses[verseNum];
            } else
            {
                return ("Not found.");
            }
        }

        /// <summary>
        /// Set the verse. Adding a new one if it doesn't exist.
        /// </summary>
        /// <param name="verseNum">Verse to update or add.</param>
        /// <param name="verseText">Verse text to set.</param>
        public void SetVerse(int verseNum, string verseText)
        {
            _verses[verseNum] = verseText;
        }
    }
}
