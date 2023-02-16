using System.IO;
using System.Linq;

namespace BibleHTMLConverter {
    public class BookParser {
        private List<string> _inputFile;
        private string _currentLine = ""; // Current line from the input file
        private int _nextLine = 0; // Index of next line to read

        /// <summary>
        /// Opens the input .html file and extract the verses.
        /// </summary>
        /// <param name="inputFile">Input .html file to parse.</param>
        public BookParser(string inputFile) {
            _inputFile = File.ReadAllText(inputFile).Split("<p>").ToList();
        }

        /// <summary>
        /// Check is there are more commands in the input file.
        /// </summary>
        /// <returns>True if the input has more commands</returns>
        public bool HasMoreLines() => _nextLine < _inputFile.Count;

        /// <summary>
        /// Reads the next line from the input and makes it the current line.
        /// Called only if HasMoreLines() is true.
        /// Initially, there is no current line.
        /// </summary>
        public void Advance() { _currentLine = _inputFile[_nextLine++]; }

        /// <summary>
        /// Cleans the file, removing empty lines, comments, and white-space.
        /// </summary>
        /// <param name="file">A list of the input file lines.</param>
        /// <returns>The cleaned input list.</returns>
        private List<string> CleanFile(List<string> file) {
            List<string> cleanFile = file.Where(line => {
                line = line.Trim();
                return line != "" && line[0] != '/' && line[0] != '*';
            }).ToList();
            cleanFile = cleanFile.Select(line => {
                string cleanLine = line.Split("//")[0]; // Removes inline comments. Anything after non-escaped double / is considered a comment.
                cleanLine = cleanLine.Trim(); // Removes whitespace around lines.
                return cleanLine;
            }).ToList();

            return cleanFile;
        }
    }
}
