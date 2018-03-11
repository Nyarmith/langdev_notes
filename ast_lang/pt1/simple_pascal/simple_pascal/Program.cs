using System;

namespace simple_pascal
{
    class tokens
    {
        public const string INTEGER = "INTEGER";
        public const string PLUS = "PLUS";
        public const string EOF = "EOF";
    }

    class Token
    {
        public string type;
        public string value;

        public Token(string type_, string value_)
        {
            type  = type_;
            value = value_;
        }

        public override string ToString()
        {
            return String.Format("Token({0},{1})",type,value);
        }
    }

    class Interpreter
    {
        private string text;
        private int pos;
        private Token current_token;
        public Interpreter(string text_)
        {
            text = text_;
            current_token = null;
        }

        //tokenizer, breaks sentence into tokens
        public Token getNextToken()
        {
            string ct = text;

            if (pos >= ct.Length)
                return new Token(tokens.EOF, null);

            char current_char = text[pos];

            if (Char.IsDigit(current_char))
            {
                Token ret = new Token(tokens.INTEGER, Char.ToString(current_char));
                pos++;
                return ret;
            }
            if (current_char == '+')
            {
                Token ret = new Token(tokens.PLUS, Char.ToString(current_char));
                pos++;
                return ret;
            }

            throw new Exception("Unrecognized symbol :" + current_token);
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

            Token op = current_token;
            eat(tokens.PLUS);

            Token right = current_token;
            eat(tokens.INTEGER);

            string result = Convert.ToString(
                Convert.ToInt32(left.value) + Convert.ToInt32(right.value)
                );

            return result;
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
    }

    // Main lessons of this part:
    // token objects, tokenizing, eat() as an "assert" type thing
    //lexer = lexical analysis = tokenizer = tokenizing = "scanner"
}
