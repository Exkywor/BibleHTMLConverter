using System.Linq;

namespace BibleHTMLConverter {
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

        public Chapter GetChapter(int chapterNum)
        {
            if (_chapters.ContainsKey(chapterNum))
            {
                return _chapters[chapterNum];
            } else
            {
                return null;
            }
        }
        public void SetChapter(int chapterNum, Chapter chapter)
        {
            _chapters[chapterNum] = chapter;
        }
    }
}
