using System.Xml;

namespace BibleHTMLConverter {
    public static class ConversionEngine {
        private static Book _book;

        public static void Convert(string inputFile)
        {
            _book = BookParser.Parse(inputFile);
            string x = "";
        }

    }
}
