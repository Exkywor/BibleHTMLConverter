using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleHTMLConverter {
    public class VMWriter {
        private StreamWriter _outputFile;
        private string _outputFilename;
        /// <summary>
        /// Creates a new output .vm file and prepares it for writing.
        /// </summary>
        /// <param name="output">Vm file name.</param>
        public VMWriter(string output) {
            _outputFilename = Path.GetFileNameWithoutExtension(output);
            _outputFile = new(output);
        }

        /// <summary>
        /// Writes a VM push command.
        /// </summary>
        /// <param name="segment">Push command: CONST, ARG, LOCAL, STATIC, THIS, THAT, POINTER, TEMP.</param>
        /// <param name="index">Index to push to.</param>
        public void WritePush(string segment, int index) {
            _outputFile.WriteLine($"push {segment} {index}");
        }

        /// <summary>
        /// Writes a VM pop command.
        /// </summary>
        /// <param name="segment">Pop command: CONST, ARG, LOCAL, STATIC, THIS, THAT, POINTER, TEMP.</param>
        /// <param name="index">Index to pop to.</param>
        public void WritePop(string segment, int index) {
            _outputFile.WriteLine($"pop {segment} {index}");
        }

        /// <summary>
        /// Writes a VM arithmetical-logical command.
        /// </summary>
        /// <param name="command">Command: ADD, SUB, NEG, EQ, GT, LT, AND, OR, NOT.</param>
        public void WriteArithmetic(string command) {
            _outputFile.WriteLine(command);
        }

        /// <summary>
        /// Writes a VM label command.
        /// </summary>
        /// <param name="label">Label's name.</param>
        public void WriteLabel(string label) {
            _outputFile.WriteLine($"label {label}");
        }

        /// <summary>
        /// Writes a VM goto command.
        /// </summary>
        /// <param name="label">Label to go to.</param>
        public void WriteGoto(string label) {
            _outputFile.WriteLine($"goto {label}");
        }

        /// <summary>
        /// Writes a VM if-goto command.
        /// </summary>
        /// <param name="label">Label to go to.</param>
        public void WriteIf(string label) {
            _outputFile.WriteLine($"if-goto {label}");
        }

        /// <summary>
        /// Writes a VM call command.
        /// </summary>
        /// <param name="name">Function/method's name.</param>
        /// <param name="nArgs">Number of arguments.</param>
        public void WriteCall(string name, int nArgs) {
            _outputFile.WriteLine($"call {name} {nArgs}");
        }

        /// <summary>
        /// Writes a VM function command.
        /// </summary>
        /// <param name="name">Function's name</param>
        /// <param name="nLocals"></param>
        public void WriteFunction(string name, int nLocals) {
            _outputFile.WriteLine($"function {_outputFilename}.{name} {nLocals}");
        }

        /// <summary>
        /// Writes a VM return command.
        /// </summary>
        public void WriteReturn() {
            _outputFile.WriteLine("return");
        }

        /// <summary>
        /// Closes the output file.
        /// </summary>
        public void Close() {
            _outputFile.Flush();
            _outputFile.Dispose();
        }
    }
}
