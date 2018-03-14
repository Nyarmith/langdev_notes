using System;

namespace pascal_with_ast
{
    /* So we're going to make something that builds a form of Intermediate-Representation(IR), in this case it's an AST.
     * A parser with no IR and just makes a single pass, evaluating expressions as soon as they're recognized are called "syntax-directed" interpreters
     * 
     * A parse-tree(sonetimes called a concrete-syntax tree) is a tree that represents the syntactic structure of a language constructed according to its grammar
     * 
     * In our previous cases, the call stack implicitly represents a parse tree
     */


    /* Main differences between ASTs and Parse trees
     * *ASTs use operators/operations as root and interior nodes, with operands as their children
     * *ASTs do not use interior nodes to represent a grammar rule, unlike the parse tree
     * *ASTs don't prepresent every detail from the real syntax (that's why they're called abstract), no rule nodes and no parentheses, for example
     * *ASTs are dense compared to a parse tree for the same language construct
     */

    class AST
    {
        public Token token;
    }

    class BinOp : AST
    {
        public AST left;
        public AST right;
        public Token op;
        public BinOp(AST left_, Token op_, AST right_)
        {
            left = left_;
            token = op = op_;
            right = right_;
        }
    }

    class Num : AST
    {
        public string value;
        public Num(Token token_)
        {
            token = token_;
            value = token_.value;
        }
    }

    class NodeVisitor
    {
        public 
    }

    class tokens
    {
        public const string INTEGER = "INTEGER";
        public const string PLUS = "PLUS";
        public const string MINUS = "MINUS";
        public const string MUL = "MUL";
        public const string DIV = "DIV";
        public const string LPAREN = "LPAREN";
        public const string RPAREN = "RPAREN";
        public const string EOF = "EOF";
    }

    class Token
    {
        public string type;
        public string value;

        public Token(string type_, string value_)
        {
            type = type_;
            value = value_;
        }

        public override string ToString()
        {
            return String.Format("Token({0},{1})", type, value);
        }
    }

    class Lexer
    {
        private string text;
        private int pos;
        private char current_char;
        public Lexer(string text_)
        {
            text = text_;
            pos = 0;
            current_char = text[pos];
        }

        /** Signal error during scanning
         */
        private void error()
        {
            throw new Exception("Invalid Character");
        }

        /** Skips whitespace characters in the current index
         */
        private void skipWhiteSpace()
        {
            while (current_char != '\0' && Char.IsWhiteSpace(current_char))
                advance();
        }

        /** Attempt to increment the scanner position
         */
        void advance()
        {
            pos++;
            if (pos >= text.Length)
                current_char = '\0';
            else
                current_char = text[pos];
        }

        /**scans the current sequence of numeric characters into an string and returns it
        */
        string integer()
        {
            string result = "";
            while (current_char != '\0' && Char.IsDigit(current_char))
            {
                result += current_char;
                advance();
            }
            return result;
        }

        /**Does the breaking into tokens
         */
        public Token getNextToken()
        {
            while (current_char != '\0')
            {
                if (Char.IsWhiteSpace(current_char))
                {
                    skipWhiteSpace();
                    continue;
                }
                if (Char.IsDigit(current_char))
                {
                    return new Token(tokens.INTEGER, integer());
                }
                if (current_char == '*')
                {
                    advance();
                    return new Token(tokens.MUL, "*");
                }
                if (current_char == '/')
                {
                    advance();
                    return new Token(tokens.DIV, "/");
                }
                if (current_char == '-')
                {
                    advance();
                    return new Token(tokens.MINUS, "-");
                }
                if (current_char == '+')
                {
                    advance();
                    return new Token(tokens.PLUS, "+");
                }
                if (current_char == '(')
                {
                    advance();
                    return new Token(tokens.LPAREN, "(");
                }
                if (current_char == ')')
                {
                    advance();
                    return new Token(tokens.RPAREN, ")");
                }
                error();
            }
            return new Token(tokens.EOF, null);
        }

    }

    class Interpreter
    {

        private Token current_token;
        private Lexer lexer;

        public Interpreter(Lexer lexer_)
        {
            lexer = lexer_;
            current_token = lexer.getNextToken();
        }

        private void error()
        {
            throw new Exception("Invalid Syntax");
        }

        //check current token, mark it as "consumed", and get next one
        private void eat(string token_type)
        {
            if (current_token.type == token_type)
                current_token = lexer.getNextToken();
            else
                error();
        }

        /** expr: term ((MUL|DIV) term)*
        */
        public AST expr()
        {
            AST node = term();

            /* ((MUL|DIV) factor)* turns into a while loop */
            string[] valid_operators = new string[] { tokens.PLUS, tokens.MINUS };
            while (Array.IndexOf(valid_operators, current_token.type) != -1)
            {
                Token t = current_token;
                if (t.type == tokens.PLUS)
                {
                    eat(tokens.PLUS);
                }
                else if (t.type == tokens.MINUS)
                {
                    eat(tokens.MINUS);
                }
                node = new BinOp(node, t, term());
            }
            return node;
        }

        /** term: factor ((MUL|DIV) factor)*
        */
        public AST term()
        {
            AST node = factor();

            string[] valid_operators = new string[] { tokens.MUL, tokens.DIV };
            while (Array.IndexOf(valid_operators, current_token.type) != -1)
            {
                Token t = current_token;
                if (t.type == tokens.MUL)
                {
                    eat(tokens.MUL);
                }
                else if (t.type == tokens.DIV)
                {
                    eat(tokens.DIV);
                }
                node = new BinOp(node, t, factor());
            }

            return node;
        }


        /** factor : INTEGER | LPAREN expr RPAREN
         */
        public AST factor()
        {
            Token tkn = current_token;
            if (tkn.type == tokens.LPAREN)
            {
                eat(tokens.LPAREN);
                AST node = expr();
                eat(tokens.RPAREN);
                return node;
            }

            else if (current_token.type == tokens.INTEGER)
            {
                eat(tokens.INTEGER);
                return new Num(tkn);
            }

            error();
            return new AST(); //should never get here
        }

        public AST parse()
        {
            return expr();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            while (true)
            {
                Console.Write("Calc>");
                string input = Console.ReadLine();

                if (input == "")
                    continue;

                Lexer lexer = new Lexer(input);
                Interpreter intrp = new Interpreter(lexer);
                Console.WriteLine(intrp.expr());
            }
        }
    }
}
