namespace BibleHTMLConverter {
    public class BibleHTMLConverter {
        static void Main(string[] args) {
            List<string> files = new();

            // Check that the file has been provided and is the correct format
            if (args.Length < 1) { throw new Exception("HTML file or directory to convert not provided.\nUsage: RVSBTConverter.cs <file.html | directory>"); }

            bool isFile = File.Exists(args[0]);

            if (isFile && !File.Exists(args[0])) {
                throw new Exception($"{args[0]} does not exist.");
            } else if (isFile && Path.GetExtension(args[0]) != ".html") {
                throw new Exception("Provided file is not a html file.");
            }

            if (isFile) { files.Add(args[0]); }
            else { files.AddRange(Directory.EnumerateFiles(args[0], "*.html", SearchOption.AllDirectories).ToList()); }

            foreach (string file in files) {
                ConversionEngine.Convert(file);
            }
        }
    }
}