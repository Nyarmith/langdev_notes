using System;

namespace pascal_with_ast
{
    /* So we're going to make something that builds a form of Intermediate-Representation(IR), in this case it's an AST.
     * A parser with no IR and just makes a single pass, evaluating expressions as soon as they're recognized are called "syntax-directed" interpreters
     */
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

        public string expr()
        {
            //Ok, we're going to do the recognition of our BNF stuff here!
            //expr: factor ((MUL|DIV) factor)*
            //factor: INTEGER
            int ret = Convert.ToInt32(term());

            /* ((MUL|DIV) factor)* turns into a while loop */
            string[] valid_operators = new string[] { tokens.PLUS, tokens.MINUS };
            while (Array.IndexOf(valid_operators, current_token.type) != -1)
            {
                Token t = current_token;
                if (t.type == tokens.PLUS)
                {
                    eat(tokens.PLUS);
                    ret += Convert.ToInt32(term());
                }
                else if (t.type == tokens.MINUS)
                {
                    eat(tokens.MINUS);
                    ret -= Convert.ToInt32(term());
                }
            }
            return Convert.ToString(ret);
        }

        public string term()
        {
            //Ok, we're going to do the recognition of our BNF stuff here!
            //expr: nest ((MUL|DIV) nest)*
            //factor: INTEGER
            int ret = Convert.ToInt32(factor());

            /* ((MUL|DIV) factor)* turns into a while loop */
            string[] valid_operators = new string[] { tokens.MUL, tokens.DIV };
            while (Array.IndexOf(valid_operators, current_token.type) != -1)
            {
                Token t = current_token;
                if (t.type == tokens.MUL)
                {
                    eat(tokens.MUL);
                    ret *= Convert.ToInt32(factor());
                }
                else if (t.type == tokens.DIV)
                {
                    eat(tokens.DIV);
                    ret /= Convert.ToInt32(factor());
                }
            }
            return Convert.ToString(ret);
        }


        /** Returns an INTEGER token value
         *  factor : INTEGER
         */
        public string factor()
        {
            string ret;
            if (current_token.type == tokens.LPAREN)
            {
                eat(tokens.LPAREN);
                ret = expr();
                eat(tokens.RPAREN);
                return ret;
            }

            else if (current_token.type == tokens.INTEGER)
            {

                ret = current_token.value;
                eat(tokens.INTEGER);
                return ret;
            }

            error();
            return ""; //should never get here
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
