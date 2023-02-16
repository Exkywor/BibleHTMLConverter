using System.Xml;

namespace BibleHTMLConverter {
    public class ConversionEngine {
        private BookParser bookParser;
        private VMWriter writer;
        private Book book;

        /// <summary>
        /// Creates a new conversion engine with the given input.
        /// </summary>
        /// <param name="inputFile">Input book in .html file.</param>
        public ConversionEngine(string inputFile) {
            bookParser = new(inputFile);

        }

    }
}
