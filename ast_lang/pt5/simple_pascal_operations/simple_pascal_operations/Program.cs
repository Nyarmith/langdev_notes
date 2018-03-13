using System;

namespace simple_pascal_operations
{

    //main lessons(now moved to front):
    //ordinarily arithmetic operators are left-associative
    //some operators have higher precedence
    //--------
    //People Usually Make  Precedence Tables to Illusrate operator Precedence, which would incldue precedence level (lower is better) and associativity
    /** e.g.
     * Precedence Level  | Associativity | Operators
     * 2                 |  left         | +,-
     * 1                 |  left         | *,/
     */

    /* How to construct a grammar from the precedence table:
     * (1) For each level of precedence define a non-terminal.
     *     The body of a production for the non-terminal should contain the arithmetic operators from that level and non-terminals for hte next higher level of precedence
     * (2) Create an additional nnon-terminal factor for basic units of expression(in our case so far it's for integers).
     *     This is used for the lowest level(aka highest precedence) oepration.
     *     This means that for N levels of precedence we will need N+1 non-terminals in total
     *
     * (remember the previous section for how we implement rules!)
     *
     * New Grammar:
     * expr   : term ((PLUS|MINUS) term)*
     * term   : factor ((MUL|DIV) factor)*
     * factor : INTEGER
     */

    /* exercise 1 was to implement this without looking at the instructor's source, now exercise 2 is to implement parentheses, here's the grammar for this
     * expr   : term ((PLUS|MINUS) term)*
     * term   : nest ((MUL|DIV) nest)*
     * nest   : (LPAREN expr RPAREN | factor)
     * factor : INTEGER
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
            int ret = Convert.ToInt32(nest());

            /* ((MUL|DIV) factor)* turns into a while loop */
            string[] valid_operators = new string[] { tokens.MUL, tokens.DIV };
            while (Array.IndexOf(valid_operators, current_token.type) != -1)
            {
                Token t = current_token;
                if (t.type == tokens.MUL)
                {
                    eat(tokens.MUL);
                    ret *= Convert.ToInt32(nest());
                }
                else if (t.type == tokens.DIV)
                {
                    eat(tokens.DIV);
                    ret /= Convert.ToInt32(nest());
                }
            }
            return Convert.ToString(ret);
        }

        public string nest()
        {
            string ret;
            if (current_token.type == tokens.LPAREN)
            {
                eat(tokens.LPAREN);
                ret = expr();
                if (current_token.type == tokens.RPAREN)
                {
                    eat(tokens.RPAREN);
                    return ret;
                }
                else
                {
                    throw new Exception("Error, unmatched parentheses!");
                }
            }

            return factor();

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
    }
}
