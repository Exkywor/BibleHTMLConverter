namespace BibleHTMLConverter
{
    public class BibleHTMLConverter
    {
        private static List<string> orderedNames = new()
        {
            "Mateo", "Marcos", "Lucas", "Juan",
            "Hechos", "Romanos",
            "1 Corintios", "2 Corintios", "Gálatas", "Efesios", "Filipenses", "Colosenses",
            "1 Tesalonicenses", "2 Tesalonicenses", "1 Timoteo", "2 Timoteo",
            "Tito", "Filemón", "Hebreos", "Santiago",
            "1 Pedro", "2 Pedro", "1 Juan", "2 Juan", "3 Juan", "Judas", "Apocalipsis"
        };

        static void Main(string[] args)
        {
            List<string> files = new();

            // Check that the file has been provided and is the correct format
            if (args.Length < 1) { throw new Exception("HTML file or directory to convert not provided.\nUsage: RVSBTConverter.cs <file.html | directory>"); }

            bool isFile = File.Exists(args[0]);

            if (isFile && !File.Exists(args[0]))
            {
                throw new Exception($"{args[0]} does not exist.");
            }
            else if (isFile && Path.GetExtension(args[0]) != ".html")
            {
                throw new Exception("Provided file is not a html file.");
            }

            if (isFile) { files.Add(args[0]); }
            else
            {
                files.AddRange(Directory.EnumerateFiles(args[0], "*.html", SearchOption.AllDirectories).ToList());
                if (files.Count == 0)
                {
                    throw new Exception("Empty directory.");
                }
            }


            BibleWriter.StartBibleWriter("test.xmm");
            foreach (string bookName in orderedNames)
            {
                string file = "";
                file = files.Find(f => Path.GetFileNameWithoutExtension(f)
                            .Equals(bookName, StringComparison.OrdinalIgnoreCase));
                Console.WriteLine(file);

                if (!string.IsNullOrWhiteSpace(file))
                {
                    Book _book = BookParser.Parse(file);
                    Console.WriteLine(_book.GetChapterList().Count());
                    BibleWriter.AddBook(_book);
                }
            }
            BibleWriter.UpdateBible();
        }
    }
}