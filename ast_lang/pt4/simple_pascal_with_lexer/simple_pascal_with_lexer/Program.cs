using System;

namespace simple_pascal_with_lexer
{
    class tokens
    {
        public const string INTEGER = "INTEGER";
        public const string PLUS = "PLUS";
        public const string MINUS = "MINUS";
        public const string MUL = "MUL";
        public const string DIV = "DIV";
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

        private string term()
        {
            Token cur = current_token;
            eat(tokens.INTEGER);
            return cur.value;
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
            int ret = Convert.ToInt32(factor());

            /* ((MUL|DIV) factor)* turns into a while loop */
            string[] valid_operators = new string[] {tokens.MUL, tokens.DIV};
            while(Array.IndexOf(valid_operators, current_token.type) != -1)
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
            Token ret = current_token;
            eat(tokens.INTEGER);
            return ret.value;
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

        //main lessons:
        // -context free grammars
        // for each statement in the BNF, left-hand side is "rule", right-hand-side is "production"
        // the rule itself is not/cannot be a terminal, the right-hand side can have terminals, of course
        // the left-hand side of the first rule can also be called a "start symbol"

        //example:
        //  expr   : factor ((MUL | DIV) factor)*
        //  factor : INTEGER

        //reads as: "An expr can be a factor, optionally followed by a multiplication or division operator followed by another factor, regex asterisk for repetition numbers being (0-n)


        //guidelines for translating gramar into a parser:
        /*
         * 1. Each rule R, defined in the grammar becomes a method with the same name, and references to that rule become a method call: R(). The body of the method follows the flow of the body of the rule using the very same guidelines
         * 2. Alternatives (a1 | a2 | aN) become an if-elif-else statement
         * 3. Optional grouping (...)* becomes a while statement
         * 4. Each token reference T becomes a call tot he method eat: eat(T)
         * 
         * This image is pretty great: https://ruslanspivak.com/lsbasi-part4/lsbasi_part4_rules.png
         *
         * 
         * Anyway, end-of-day lesson: tokenizing and separating abstraction is really useful. Using production rules is mandatory for good clean code. Using the right abstraction here == winning!
         */
    }
}
