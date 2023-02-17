using System.Text.RegularExpressions;

namespace BibleHTMLConverter
{
    public static class BookParser
    {
        private static string _inputFile = "";
        private static List<string> _lines;
        private static string _currentLine = ""; // Current line from the input file
        private static int _nextLine = 0; // Index of next line to read

        private static Book _book;
        private static int _currChapNum; // Current chapter number
        private static Chapter _currChap; // Current chapter to which to add verses

        /// <summary>
        /// Parse a bible book contained in <p> tags, storing the verses and chapters in a new Book.
        /// </summary>
        /// <param name="inputFile">HTML file containing the book.</param>
        /// <returns>Book result of the parsed file.</returns>
        public static Book Parse(string inputFile)
        {
            _inputFile = inputFile;
            PrepareBook();

            while (HasMoreLines())
            {
                Advance();
                ParseLine(_currentLine);
            }

            // Add last chapter if missing
            if (!_book.GetChapterList().Contains(_currChapNum))
            {
                _book.SetChapter(_currChapNum, _currChap);
            }

            return _book;
        }

        /// <summary>
        /// Make a new book and reset the variables to fill it.
        /// </summary>
        private static void PrepareBook()
        {
            string file = File.ReadAllText(_inputFile);
            _lines = file.Split("<p>").ToList();
            _book = new Book(Path.GetFileNameWithoutExtension(_inputFile));
            _currChapNum = 0;
        }

        /// <summary>
        /// Check is there are more commands in the input file.
        /// </summary>
        /// <returns>True if the input has more commands</returns>
        private static bool HasMoreLines() => _nextLine < _lines.Count;

        /// <summary>
        /// Reads the next line from the input and makes it the current line.
        /// Called only if HasMoreLines() is true.
        /// Initially, there is no current line.
        /// </summary>
        private static void Advance()
        {
            if (HasMoreLines())
            {
                _currentLine = _lines[_nextLine++];

                // Skip useless lines
                if (string.IsNullOrEmpty(_currentLine) || _currentLine.StartsWith("&nbsp;"))
                {
                    Advance();
                }
            }
        }

        /// <summary>
        /// Parse a line, which can be a chapter title or a verse.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        private static void ParseLine(string line)
        {
            line = CleanLine(line);
            if (!string.IsNullOrWhiteSpace(line))
            {
                if (line.Contains("capítulo", StringComparison.OrdinalIgnoreCase))
                {
                    ParseChapter(line);
                }
                else if (char.IsDigit(line[0]))
                {
                    ParseVerse(line);
                }
            }
        }

        /// <summary>
        /// Clean a line of tags and extra characters.
        /// </summary>
        /// <param name="line">Line to clean.</param>
        /// <returns>Cleaned line.</returns>
        private static string CleanLine(string line)
        {
            // Remove tags
            line = Regex.Replace(line, "<[^>]*>", "");

            // Remove &nbsp; characters
            line = line.Replace("&nbsp;", " ");

            if (!string.IsNullOrWhiteSpace(line))
            {
                return line.Trim();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Parse a chapter line, storing the chapter number in a Book.
        /// </summary>
        /// <param name="line">Line containing the chapter.</param>
        private static void ParseChapter(string line)
        {
            // Reached a new chapter, so we store the current one, if it exists
            if (_currChapNum != 0)
            {
                _book.SetChapter(_currChapNum, _currChap);
            }

            int chapter = TokenizeChapter(line);
            if (chapter != 0)
            {
                _currChap = new();
                _currChapNum = chapter;
            }
        }

        /// <summary>
        /// Parse a verse line, storing it in the current chapter.
        /// </summary>
        /// <param name="line">Line containing the verse.</param>
        private static void ParseVerse(string line)
        {
            (int verseNumber, string verseText) = TokenizeVerse(line);

            if (_currChap != null && verseNumber != 0)
            {
                _currChap.SetVerse(verseNumber, verseText);
            }
        }

        /// <summary>
        /// Tokenize a chapter line, extracting the chapter number.
        /// </summary>
        /// <param name="line">Chapter line to tokenize.</param>
        /// <returns>Chapter number if it exists, 0 if it doesn't.</returns>
        private static int TokenizeChapter(string line)
        {
            string chapter = line.Split(" ").Last();
            if (int.TryParse(chapter, out int chap))
            {
                return chap;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Tokenize a verse line, separating the verse number from the text.
        /// </summary>
        /// <param name="line">Verse line to tokenize.</param>
        /// <returns>Tokenized verse line. (Verse number, Verse text).</returns>
        private static (int, string) TokenizeVerse(string line)
        {
            int firstSpaceIndex = line.IndexOf(" ");
            string verseNumber = line[..firstSpaceIndex]; // Verse number
            string verseText = line[(firstSpaceIndex + 1)..].Trim(); // Verse text

            if (int.TryParse(verseNumber, out int num))
            {
                return (num, verseText);
            }
            else
            {
                return (0, "");
            }
        }
    }
}
