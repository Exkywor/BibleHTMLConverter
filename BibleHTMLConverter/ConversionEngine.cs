using System.Xml;

namespace BibleHTMLConverter {
    public static class ConversionEngine {
        private static Book _book;

        public static void Convert(string inputFile)
        {
            _book = BookParser.Parse(inputFile);
            BibleWriter.StartBibleWriter(inputFile);
            BibleWriter.AddBook(_book);
            BibleWriter.UpdateBible();
        }

    }
}
