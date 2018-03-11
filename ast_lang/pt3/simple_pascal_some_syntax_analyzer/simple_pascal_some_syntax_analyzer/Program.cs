using System;

namespace simple_pascal_some_syntax_analyzer
{
    class tokens
    {
        public const string INTEGER = "INTEGER";
        public const string PLUS = "PLUS";
        public const string MINUS = "MINUS";
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

    class Interpreter
    {
        private string text;
        private int pos;
        private Token current_token;
        private char current_char;


        public Interpreter(string text_)
        {
            text = text_;
            pos = 0;
            current_token = null;
            current_char = text[pos];
        }

        //tokenizer, breaks sentence into tokens
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
                    Token ret = new Token(tokens.INTEGER, Char.ToString(current_char));
                    advance();
                    return ret;
                }
                if (current_char == '+')
                {
                    Token ret = new Token(tokens.PLUS, Char.ToString(current_char));
                    advance();
                    return ret;
                }
                if (current_char == '-')
                {
                    Token ret = new Token(tokens.MINUS, Char.ToString(current_char));
                    advance();
                    return ret;
                }
                throw new Exception("Unrecognized symbol :" + current_token);
            }
            return new Token(tokens.EOF, null);
        }

        private void skipWhiteSpace()
        {
            while (current_char != '\0' && Char.IsWhiteSpace(current_char))
                advance();
        }

        void advance()
        {
            pos++;
            if (pos >= text.Length)
                current_char = '\0';
            else
                current_char = text[pos];
        }

        int integer()
        {
            string result = "";
            while (current_char != '\0' && Char.IsDigit(current_char))
            {
                result += current_char;
                advance();
            }
            return current_char;
        }

        public string term()
        {
            Token cur = current_token;
            eat(tokens.INTEGER);
            return cur.value;
        }

        //check current token, mark it as "consumed", and get next one
        public void eat(string token_type)
        {
            if (current_token.type == token_type)
                current_token = getNextToken();
            else
                throw new Exception("Mismatched tokens in eat(type)");
        }

        public string expr()
        {
            current_token = getNextToken();

            int res = Convert.ToInt32(term());

            while (Array.IndexOf(new String[] { tokens.PLUS, tokens.MINUS }, current_token.type) != -1)
            {
                Token op = current_token;
                if (op.type == tokens.PLUS)
                {
                    eat(tokens.PLUS);
                    res += Convert.ToInt32(term());
                }
                else
                {
                    eat(tokens.MINUS);
                    res -= Convert.ToInt32(term());
                }
            }
            return Convert.ToString(res);
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

                Interpreter intrp = new Interpreter(input);
                Console.WriteLine(intrp.expr());
            }
        }

        //main lessons:
        // -parsing is also syntax analysis, thus a parser/scanner also functions as a syntax anaylzer
        // -syntax diagram specifically referes to which statements are valid and which aren't
        // -a term, for our current purposes, is just an integer
        // -uh you should make syntax diagrams and make your parser follow the diagram with correct terminology

        // * To be clear, the parser just recognizes the structure making sure that it corresponds to the language's specification, and the interpreteractually evaluates the expression sonce the parser has successfully recognized(parsed) it
    }
}
