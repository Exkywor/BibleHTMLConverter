using System.Linq;

namespace BibleHTMLConverter {
    /// <summary>
    /// Represents a Book.
    /// Chapter numbers are stored as keys to account for possible errors in the source files.
    /// </summary>
    public class Book {
        private string _name;
        private Dictionary<int, Chapter> _chapters; 

        /// <summary>
        /// Creates a new Book.
        /// </summary>
        public Book(string name) {
            _name = name;
            _chapters = new();
        }

        public string GetBookName() => _name;
        public void SetBookName(string name) => _name = name;

        /// <summary>
        /// Get the input chapter.
        /// </summary>
        /// <param name="chapterNum">Chapter to find.</param>
        /// <returns>The requested chapter. Null if not found.</returns>
        public Chapter? GetChapter(int chapterNum)
        {
            if (_chapters.ContainsKey(chapterNum))
            {
                return _chapters[chapterNum];
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Set the chapter. Adding a new one if it doesn't exist.
        /// </summary>
        /// <param name="chapterNum">Chpater to update or add.</param>
        /// <param name="chapter">Chapter to set.</param>
        public void SetChapter(int chapterNum, Chapter chapter)
        {
            _chapters[chapterNum] = chapter;
        }

        /// <summary>
        /// Get a list of chapters contained in the book.
        /// </summary>
        /// <returns>List of chapters.</returns>
        public List<int> GetChapterList()
        {
            return _chapters.Keys.ToList();
        }
    }
}
