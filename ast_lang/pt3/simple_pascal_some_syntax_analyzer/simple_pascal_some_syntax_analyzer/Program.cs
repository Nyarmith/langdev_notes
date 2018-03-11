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

            Token left = current_token;
            eat(tokens.INTEGER);  //we expect the token to be an integer

            /*
            Token op = current_token;
            if (op.type == tokens.PLUS)
                eat(tokens.PLUS);
            else
                eat(tokens.MINUS);

            Token right = current_token;
            eat(tokens.INTEGER);

            string result;
            if (op.type == tokens.MINUS)
                result = Convert.ToString(
                Convert.ToInt32(left.value) - Convert.ToInt32(right.value)
                );
            else
                result = Convert.ToString(
                Convert.ToInt32(left.value) + Convert.ToInt32(right.value)
                );
                */
            int running_val = Convert.ToInt32(left.value);
            while (current_token.type != tokens.EOF)
            {
                Token op = current_token;
                if (op.type == tokens.PLUS)
                    eat(tokens.PLUS);
                else
                    eat(tokens.MINUS);

                Token nextNum = current_token;
                eat(tokens.INTEGER);
                if (op.type == tokens.MINUS)
                    running_val -= Convert.ToInt32(nextNum.value);
                else
                    running_val += Convert.ToInt32(nextNum.value);
            }

            return Convert.ToString(running_val);
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
    }
}
