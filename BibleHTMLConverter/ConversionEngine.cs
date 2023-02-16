using System.Xml;

namespace BibleHTMLConverter {
    public class ConversionEngine {
        private BookParser tokenizer;
        private VMWriter writer;
        private Book symbolTable;

        private string className;
        private bool isLastSubroutineDecVoid; // Used to inform CompileReturn without passing arguments like 5 levels down
        private int whileCounter;
        private int ifCounter;

        /// <summary>
        /// Creates a new compilation engine with the given input and output.
        /// The next routine called must be CompileClass.
        /// </summary>
        /// <param name="inputFile">Input book in .html file.</param>
        public ConversionEngine(string inputFile) {
            tokenizer = new(inputFile);
            symbolTable = new();
            isLastSubroutineDecVoid = false;
            whileCounter = -1;
            ifCounter = -1;
            className = outputFilename;

            writer = new VMWriter(Path.Combine(Path.GetDirectoryName(inputFile), $"{outputFilename}.vm"));

            CompileClass();

            writer.Close();
        }


        /// <summary>
        /// Compiles a complete class.
        /// </summary>
        public void CompileClass() {
            symbolTable = new();
            isLastSubroutineDecVoid = false;
            whileCounter = -1;
            ifCounter = -1;

            tokenizer.Advance();

            // class
            tokenizer.Advance();

            // className
            tokenizer.Advance();

            // {
            tokenizer.Advance();

            // Compile classVarDecs until we reach the first subroutineDec or the end of the class
            while (!((tokenizer.TokenType() == "keyword" && tokenizer.KeyWord() is "constructor" or "function" or "method")
                 || (tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == "}"))) {
                CompileClassVarDec();
            }

            // Compile subroutineDec until we reach the end of the class
            while (!(tokenizer.TokenType() == "symbol" && tokenizer.Symbol() is "}" or "{")) {
                CompileSubroutineDec();
            }

            // }
        }

        /// <summary>
        /// Compiles a static variable declaration, or a field declaration.
        /// </summary>
        public void CompileClassVarDec() {
            // static | field
            string kind = tokenizer.KeyWord();
            tokenizer.Advance();

            // type
            string type = tokenizer.TokenType() == "keyword" ? tokenizer.KeyWord() : tokenizer.Identifier();
            tokenizer.Advance();

            // varName
            List<string> names = new(){ tokenizer.Identifier() };
            tokenizer.Advance();

            // (, varName)*
            while (!(tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == ";")) {
                // ,
                tokenizer.Advance();

                // varName
                names.Add(tokenizer.Identifier());
                tokenizer.Advance();
            }

            // ;
            tokenizer.Advance();

            // ADD the parsed vars to the SymbolTable, and write them to the .xml
            foreach (string name in names) { symbolTable.Define(name, type, kind); }
        }

        /// <summary>
        /// Compiles a complete method, function, or constructor.
        /// </summary>
        public void CompileSubroutineDec() {
            symbolTable.StartSubroutine(tokenizer.KeyWord() == "method");

            // constructor | function | method
            string funType = tokenizer.KeyWord();
            tokenizer.Advance();

            // void | type
            if (tokenizer.TokenType() == "keyword") { // normal types
                isLastSubroutineDecVoid = tokenizer.KeyWord() == "void";
            }
            else { // class types
                isLastSubroutineDecVoid = false;
            }
            tokenizer.Advance();

            // subroutineName
            string subroutineName = tokenizer.Identifier();
            tokenizer.Advance();

            // (
            tokenizer.Advance();

            // parameterList
            CompileParameterList();

            // )
            tokenizer.Advance();

            // subroutineBody
            CompileSubroutineVarDec();

            writer.WriteFunction(subroutineName, symbolTable.VarCount("local"));

            if (funType == "constructor") {
                writer.WritePush("constant", symbolTable.VarCount("field"));
                writer.WriteCall("Memory.alloc", 1); // Initialize memory block for the object
                writer.WritePop("pointer", 0); // Anchor "this" at the returned base address
            } else if (funType == "method") { // Anchor the provided object base address
                writer.WritePush("argument", 0);
                writer.WritePop("pointer", 0);
            }

            // statements
            // NOTE: We brought this out of the CompileSubroutineBody in order to compile after the function declaration is compiled,
            //       after the number of locals declared is figured out.
            CompileStatements();

            // }
            tokenizer.Advance();
        }

        /// <summary>
        /// Compiles a (possibly empty) paramater list. Does not handle enclosing "()".
        /// </summary>
        public void CompileParameterList() {
            // Parameter list is not ), meaning it's not empty
            if (tokenizer.TokenType() != "symbol") {
                Dictionary<string, string> args = new(); // Dictionary of Name, Type

                // type
                string tempType = tokenizer.TokenType() == "keyword" ? tokenizer.KeyWord() : tokenizer.Identifier();
                tokenizer.Advance();

                // varName
                string tempName = tokenizer.Identifier();
                tokenizer.Advance();

                args.Add(tempName, tempType);

                // (, type varName)*
                while (!(tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == ")")) {
                    // ,
                    tokenizer.Advance();

                    // type
                    tempType = tokenizer.KeyWord();
                    tokenizer.Advance();

                    // varName
                    tempName = tokenizer.Identifier();
                    tokenizer.Advance();

                    args.Add(tempName, tempType);
                }

                foreach (var (name, type) in args) {
                    symbolTable.Define(name, type, "argument");
                }
            }
        }

        /// <summary>
        /// Compiles a soubroutine's var declaration.
        /// </summary>
        public void CompileSubroutineVarDec() {
            // {
            tokenizer.Advance();

            // vardDec*
            while (tokenizer.TokenType() == "keyword" && tokenizer.KeyWord() == "local") {
                CompileVarDec();
            }
        }

        /// <summary>
        /// Compiles a var declaration.
        /// </summary>
        public void CompileVarDec() {
            // var
            string kind = tokenizer.KeyWord();
            tokenizer.Advance();

            // type
            string type = tokenizer.TokenType() == "keyword" ? tokenizer.KeyWord() : tokenizer.Identifier();
            tokenizer.Advance();

            // varName
            List<string> names = new(){ tokenizer.Identifier() };
            tokenizer.Advance();

            // (, varName)*
            while (!(tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == ";")) {
                // ,
                tokenizer.Advance();

                // varName
                names.Add(tokenizer.Identifier());
                tokenizer.Advance();
            }

            // ;
            tokenizer.Advance();

            // ADD the parsed vars to the SymbolTable
            foreach (string name in names) { symbolTable.Define(name, type, kind); }
        }

        /// <summary>
        /// Compiles a sequence of statements. Does not handle enclosing "{}".
        /// </summary>
        public void CompileStatements() {
            // Token is not }, meaning there are more statements left
            while (tokenizer.TokenType() != "symbol") {
                switch (tokenizer.KeyWord()) {
                    case "let":
                        CompileLet();
                        break;
                    case "if":
                        CompileIf();
                        break;
                    case "while":
                        CompileWhile();
                        break;
                    case "do":
                        CompileDo();
                        break;
                    case "return":
                        CompileReturn();
                        break;
                }
            }
        }

        /// <summary>
        /// Compiles a let statement.
        /// </summary>
        public void CompileLet() {
            bool isArray = false;

            // let
            tokenizer.Advance();

            // varName
            string varName = tokenizer.Identifier();
            string kind = symbolTable.KindOf(varName);
            if (kind == "field") { kind = "this"; } // We'll pop into "this"
            tokenizer.Advance();

            // [ expression ]?
            if (tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == "[") {
                // [
                isArray = true;
                tokenizer.Advance();

                writer.WritePush(kind, symbolTable.IndexOf(varName)); // Push the arr's base address

                // expression
                CompileExpression();

                writer.WriteArithmetic("add"); // Calculate the address we need to pop into

                // ]
                tokenizer.Advance();
            }

            // =
            tokenizer.Advance();

            // expression
            CompileExpression();

            if (isArray) {
                writer.WritePop("temp", 0);
                writer.WritePop("pointer", 1);
                writer.WritePush("temp", 0);
                writer.WritePop("that", 0);
            } else {
                writer.WritePop(kind, symbolTable.IndexOf(varName));
            }

            // ;
            tokenizer.Advance();
        }

        /// <summary>
        /// Compiles an if statement, possibly with a trailing else clause.
        /// </summary>
        public void CompileIf() {
            int currIfCounter = ++ifCounter; // Used to avoid issues with nested ifs.

            // if
            tokenizer.Advance();

            // (
            tokenizer.Advance();

            // expression
            CompileExpression();

            // )
            tokenizer.Advance();

            writer.WriteIf($"IF_TRUE{currIfCounter}");
            writer.WriteGoto($"IF_FALSE{currIfCounter}");

            // {
            tokenizer.Advance();

            writer.WriteLabel($"IF_TRUE{currIfCounter}");

            // Statements
            CompileStatements();

            writer.WriteGoto($"IF_END{currIfCounter}");

            // }
            tokenizer.Advance();

            // We write the false label regardless of else. Otherwise we would need to do look-ahead to
            // determine wheter to write IF_FALSE goto.
            writer.WriteLabel($"IF_FALSE{currIfCounter}"); 
            if (tokenizer.TokenType() == "keyword" && tokenizer.KeyWord() == "else") {
                // else
                tokenizer.Advance();

                // {
                tokenizer.Advance();

                // Statements
                CompileStatements();

                // }
                tokenizer.Advance();
            }

            writer.WriteLabel($"IF_END{currIfCounter}"); 
        }

        /// <summary>
        /// Compiles a while statement.
        /// </summary>
        public void CompileWhile() {
            int currWhileCounter = ++whileCounter; // Used to avoid issues with nested whiles.

            // while
            writer.WriteLabel($"WHILE_EXP{currWhileCounter}");
            tokenizer.Advance();

            // (
            tokenizer.Advance();

            // expression
            CompileExpression();

            // )
            tokenizer.Advance();

            writer.WriteArithmetic("not");
            writer.WriteIf($"WHILE_END{currWhileCounter}");

            // {
            tokenizer.Advance();

            // Statements
            CompileStatements();

            // }
            tokenizer.Advance();

            writer.WriteGoto($"WHILE_EXP{currWhileCounter}");
            writer.WriteLabel($"WHILE_END{currWhileCounter}");
        }

        /// <summary>
        /// Compiles a do statement.
        /// </summary>
        public void CompileDo() {
            // do
            tokenizer.Advance();

            // subroutineCall
            CompileSubroutineCall();

            // ;
            tokenizer.Advance();

            writer.WritePop("temp", 0);
        }

        /// <summary>
        /// Compiles a subroutine call.
        /// Does not have Starting/End elements, nor ; at the end.
        /// </summary>
        /// <param name="writeFirstName">
        /// Whether to compile the name of the call or not. Used to remove the need to look-ahead.
        /// This means that the name of the subroutine has already been written to the xml, and that the next token is . or (
        /// </param>
        /// <param name="subroutineName">Subroutine name. Empty when writeFirstName is true.</param>
        public void CompileSubroutineCall(bool writeFirstName = true, string subroutineName = "") {
            int nArgs = 0;
            bool isMethod = false;

            if (writeFirstName) {
                // subroutineName | (className | varName)
                subroutineName = subroutineName+=tokenizer.Identifier(); // 
                tokenizer.Advance();
            } 

            // We're building the subroutine call. So far we have SOMETHING.
            // We need to know if SOMETHING is a class name or an object.
            // If it's an object, it'll be in the symbol table. Otherwise we'll treat it as a class.
            int idx = symbolTable.IndexOf(subroutineName);
            isMethod = idx >= 0;

            // subroutineName
            if (tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == "(") {
                // We know it's a method call since only methods are not called with a class or object name
                isMethod = true; // We forcefully tell the compiler that we are in a method.
                string kind = symbolTable.KindOf(subroutineName);
                if (kind == "field") { kind = "this"; } // We'll push "this"
                subroutineName = $"{className}.{subroutineName}"; // We replace distance() with Point.distance()
                writer.WritePush("pointer", 0); // We push the object's address.

                // (
                tokenizer.Advance();

                nArgs = CompileExpressionList();

                // )
                tokenizer.Advance();
            } else { // className | varName
                if (isMethod) { // It's an object!
                    string kind = symbolTable.KindOf(subroutineName);
                    if (kind == "field") { kind = "this"; } // We'll push "this"
                    subroutineName = symbolTable.TypeOf(subroutineName); // We replace "p1.distance()" with "Point.distance(p1)"
                    writer.WritePush(kind, idx); // We push the object's address.
                }

                // .
                subroutineName += tokenizer.Symbol();
                tokenizer.Advance();

                // subroutineName
                subroutineName += tokenizer.Identifier();
                tokenizer.Advance();

                // (
                tokenizer.Advance();

                nArgs = CompileExpressionList();

                // )
                tokenizer.Advance();
            }

            writer.WriteCall(subroutineName, isMethod ? nArgs+1 : nArgs); // Implicit "this" if method
        }

        /// <summary>
        /// Compiles a return statement.
        /// </summary>
        public void CompileReturn() {
            // return
            tokenizer.Advance();

            // expression?
            if (!(tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == ";")) {
                CompileExpression();
            }

            // ;
            tokenizer.Advance();

            if (isLastSubroutineDecVoid) { writer.WritePush("constant", 0); }
            writer.WriteReturn();
        }

        /// <summary>
        /// Compiles an expression.
        /// </summary>
        public void CompileExpression() {
            // term
            CompileTerm();

            // (op term)*
            while (tokenizer.TokenType() == "symbol"
                && tokenizer.Symbol() is "+" or "-" or "*" or "/" or "&" or "|" or "<" or ">" or "=") {

                // op
                string op = tokenizer.Symbol();
                tokenizer.Advance();

                // term
                CompileTerm();

                switch (op) {
                    case "+":
                        writer.WriteArithmetic("add");
                        break;
                    case "-":
                        writer.WriteArithmetic("sub");
                        break;
                    case "*":
                        writer.WriteCall("Math.multiply", 2);
                        break;
                    case "/":
                        writer.WriteCall("Math.divide", 2);
                        break;
                    case "&":
                        writer.WriteArithmetic("and");
                        break;
                    case "|":
                        writer.WriteArithmetic("or");
                        break;
                    case "<":
                        writer.WriteArithmetic("lt");
                        break;
                    case ">":
                        writer.WriteArithmetic("gt");
                        break;
                    case "=":
                        writer.WriteArithmetic("eq");
                        break;
                }
            }
        }

        /// <summary>
        /// Compiles a term.
        /// If the current token is an identifier, the routine must distinguish between a variable,
        /// an array entry, or a subroutine call.
        /// A single look-ahead token, wich may be one of "[", "(", or ".", suffices to distinguish
        /// between the possibilities. Any other token is not part of this term and should not be
        /// advanced over.
        /// </summary>
        public void CompileTerm() {
            switch (tokenizer.TokenType()) {
                case "integerConstant": // integerConstant
                    writer.WritePush("constant", tokenizer.IntVal());
                    tokenizer.Advance();
                    break;
                case "stringConstant": // stringConstant
                    string str = tokenizer.StringVal();
                    writer.WritePush("constant", str.Length);
                    writer.WriteCall("String.new", 1);
                    foreach (char c in str) {
                        writer.WritePush("constant", c);
                        writer.WriteCall("String.appendChar", 2);
                    }

                    tokenizer.Advance();
                    break;
                case "keyword": // keywordConstant: true, false, null, this)
                    switch (tokenizer.KeyWord()) {
                        case "true":
                            writer.WritePush("constant", 1);
                            writer.WriteArithmetic("neg");
                            break;
                        case "false":
                            writer.WritePush("constant", 0);
                            break;
                        case "null":
                            writer.WritePush("constant", 0);
                            break;
                        case "this":
                            writer.WritePush("pointer", 0);
                            break;
                    }
                    tokenizer.Advance();
                    break;
                case "identifier": // varName | varName[expression] | subroutineCall
                    // varName | subroutineCall
                    string name = tokenizer.Identifier();
                    string kind = symbolTable.KindOf(name);
                    // We'll assume it's a subroutine call if the identifier is not in the symbol table
                    bool isSubroutineCall = !(kind is "local" or "argument" or "field" or "static");
                    tokenizer.Advance();
                    // We check if the identifier is in the symbol table and the next element is ., meaning it's
                    // a method call upon an object
                    if (tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == ".") { isSubroutineCall = true; }

                    // subroutine call
                    if (isSubroutineCall) {
                        CompileSubroutineCall(false, name); // Compiles subroutine, without writing the name
                    } else {
                        if (kind == "field") { kind = "this"; }
                        bool isArray = false;

                        // [expression]
                        if (tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == "[") {
                            // [
                            isArray = true;
                            tokenizer.Advance();

                            // expression
                            CompileExpression();

                            // ]
                            tokenizer.Advance();
                        }

                        // We know that it's not a subroutine call, and it's not a keyword, so it must be a var.
                        writer.WritePush(kind, symbolTable.IndexOf(name));

                        if (isArray) {
                            writer.WriteArithmetic("add"); // Get base address + expression offset
                            writer.WritePop("pointer", 1); // Set that to the proper segment
                            writer.WritePush("that", 0); // Push the value that "that" points to
                        } 
                    }

                    break;
                case "symbol": // (expression) | unaryOp term
                    if (tokenizer.Symbol() == "(") {
                        // (
                        tokenizer.Advance();

                        // expression
                        CompileExpression();

                        // )
                        tokenizer.Advance();
                    }
                    else {
                        // unaryOp
                        // outputFile.WriteElementString(tokenizer.TokenType(), tokenizer.Symbol());
                        string unaryOp = tokenizer.Symbol(); // We'll call this after we evaluate the term.
                        tokenizer.Advance();

                        // term
                        CompileTerm();

                        switch (unaryOp) {
                            case "-":
                                writer.WriteArithmetic("neg");
                                break;
                            case "~":
                                writer.WriteArithmetic("not");
                                break;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Compiles a (possibly empty) comma-separated list of expressions.
        /// </summary>
        /// <returns>Number of expressions</returns>
        public int CompileExpressionList() {
            int nExp = 0;

            // Expression list is not ), meaning it's not empty
            if (!(tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == ")")) {
                CompileExpression();
                nExp++;

                // (, expression)*
                while (!(tokenizer.TokenType() == "symbol" && tokenizer.Symbol() == ")")) {
                    // ,
                    tokenizer.Advance();

                    CompileExpression();
                    nExp++;
                }
            }

            return nExp;
        }
    }
}
