using System;
using System.Collections.Generic;

namespace pascal_ast_stmts
{

    /* Now we're implementing statements, or the concept of a statement.
     *
     *
     */
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------

    //====Definition of language's tokens====
    class tokens
    {
        public const string INTEGER = "INTEGER";
        public const string PLUS = "PLUS";
        public const string MINUS = "MINUS";
        public const string MUL = "MUL";
        public const string DIV = "DIV";
        public const string LPAREN = "LPAREN";
        public const string RPAREN = "RPAREN";
        public const string ASSIGN = "ASSIGN";
        public const string SEMI = "SEMI";
        public const string BEGIN = "BEGIN";
        public const string END = "END";
        public const string ID = "ID";
        public const string DOT = "DOT";
        public const string EOF = "EOF";
        public static Dictionary<String, Token> RESERVED = new Dictionary<string, Token> { { "BEGIN", new Token("BEGIN", "BEGIN") }, { "END", new Token("END", "END") } };
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

    // visitor pattern for AST nodes
    interface NodeVisitor
    {
        string visit(AST node);
        string visit(BinOp node);
        string visit(UnaryOp node);
        string visit(Num node);
        string visit(Compound node);
        string visit(Assign node);
        string visit(Var node);
        string visit(NoOp node);
    }

    interface Visitable
    {
        string accept(NodeVisitor visitor);
    }

    class AST : Visitable
    {
        public Token token;

        public virtual string accept(NodeVisitor visitor)
        {
            return visitor.visit(this);
        }
    }

    class UnaryOp : AST
    {
        public AST value;
        public Token op;

        public UnaryOp(Token op_, AST value_)
        {
            op = op_;
            value = value_;
        }
        public override string accept(NodeVisitor visitor)
        {
            return visitor.visit(this);
        }
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
        public override string accept(NodeVisitor visitor)
        {
            return visitor.visit(this);
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
        public override string accept(NodeVisitor visitor)
        {
            return visitor.visit(this);
        }
    }

    class Compound : AST
    {
        public List<AST> children;
    }

    class Assign : AST
    {
        public Var left;
        public AST right;
        public Token op;
        public Assign(Var left_, Token op_, AST right_)
        {
            left  = left_;
            op    = op_;
            right = right_;
            token = op;
        }
    }

    class Var : AST
    {
        public string value;
        public Var(Token token_)
        {
            token = token_;
            value = token.value;
        }
    }

    class NoOp : AST
    {
    }


    //================================
    //==== Lexer Definition ====
    //================================
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

        // Signal error during scanning
        private void error()
        {
            throw new Exception("Invalid Character");
        }

        // Skips whitespace characters in the current index
        private void skipWhiteSpace()
        {
            while (current_char != '\0' && Char.IsWhiteSpace(current_char))
                advance();
        }

        // Attempt to increment the scanner position
        void advance()
        {
            pos++;
            if (pos >= text.Length)
                current_char = '\0';
            else
                current_char = text[pos];
        }

        char peek()
        {
            int peek_pos = pos + 1;
            if (peek_pos >= text.Length)
                return '\0';

            return text[peek_pos];
        }


        //handles identifiers and reserved keywords
        Token _id()
        {
            string result = "";
            while (current_char != '\0' && Char.IsLetterOrDigit(current_char))
            {
                result += current_char;
                advance();
            }
            if (tokens.RESERVED.ContainsKey(result))
            {
                return tokens.RESERVED[result];
            }
            return new Token(tokens.ID, result);
        }

        // Scans the current sequence of numeric characters into an string and returns it
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

        // Does the breaking into tokens
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

                //New Tokens In Lexer
                if (Char.IsLetterOrDigit(current_char))
                {
                    return _id();
                }
                if (current_char == ':' && peek() == '=')
                {
                    advance();
                    advance();
                    return new Token(tokens.ASSIGN, ":=");
                }
                if (current_char == ';')
                {
                    advance();
                    return new Token(tokens.SEMI, ";");
                }
                if (current_char == '.')
                {
                    advance();
                    return new Token(tokens.DOT, ".");
                }
                error();
            }
            return new Token(tokens.EOF, null);
        }
    }

    //================================
    //==== Parser Definition ====
    //================================

    /* Grammar:
     *   program              : compound_statement DOT
     *   compound_statement   : BEGIN statement_list END
     *   statement_list       : statement | statement SEMI statement_list
     *   statement            : compound_statement | assignment_statement | empty
     *   assignment_statement : variable ASSIGN expr
     *   variable             : ID
     *   empty                :
     *   expr                 : term ((PLUS|MINUS) term)*
     *   term                 : term ((MUL|DIV) term)*
     *   factor               : (PLUS|MINUS) factor | INTEGER | LPAREN expr RPAREN | variable
     */
    class Parser
    {
        private Token current_token;
        private Lexer lexer;

        public Parser(Lexer lexer_)
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

        // program : compound_statement DOT
        public AST program()
        {
            AST node = compound_statement();
            Token t = current_token;
            eat(tokens.DOT);
            return node;
        }

        // compound_statement : BEGIN statement_list END
        public AST compound_statement()
        {
            eat(tokens.BEGIN);
            List<AST> nodes = statement_list();
            eat(tokens.END);

            Compound root = new Compound();

            foreach (AST node in nodes)
            {
                root.children.Add(node);
            }

            return root;
        }

        // statement_list : statement | statement SEMI statement_list
        public List<AST> statement_list()
        {
            AST node = statement();
            List<AST> ret = new List<AST>() { node };

            while (current_token.type == tokens.SEMI)
            {
                eat(tokens.SEMI);
                ret.Add(statement());
            }

            if (current_token.type == tokens.ID)
                error();

            return ret;
        }

        // statement : compound_statement | assignment_statement | empty
        AST statement()
        {
            AST ret;
            if (current_token.type == tokens.BEGIN)
                ret = compound_statement();
            else if (current_token.type == tokens.ID)
                ret = assignment_statement();
            else
                ret = empty();

            return ret;
        }

        // assignment_statement : variable ASSIGN expr
        AST assignment_statement()
        {
            Var left = variable();
            Token tkn = current_token;
            eat(tokens.ASSIGN);
            AST right = expr();
            return new Assign(left, tkn, right);
        }

        // variable : ID
        Var variable()
        {
            Var ret = new Var(current_token);
            eat(tokens.ID);
            return ret;
        }

        // empty :
        // an empty production
        AST empty()
        {
            return new NoOp();
        }

        // expr: term ((PLUS|MINUS) term)*
        public AST expr()
        {
            AST node = term();

            /* ((PLUS|MINUS) term)* turns into a while loop */
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

        // term: factor ((MUL|DIV) factor)*
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


        // factor : (PLUS|MINUS) factor | INTEGER | LPAREN expr RPAREN | variable
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
            else if (current_token.type == tokens.PLUS)
            {
                eat(tokens.PLUS);
                return new UnaryOp(tkn, factor());
            }
            else if (current_token.type == tokens.MINUS)
            {
                eat(tokens.MINUS);
                return new UnaryOp(tkn, factor());
            }
            else
            {
                return variable();
            }

            error();
            return new AST(); //should never get here
        }

        public AST parse()
        {
            AST node = program();
            if (current_token.type != tokens.EOF)
                error();
            return node;
        }
    }

    //================================
    //==== Interpreter Definition ====
    //================================
    class Interpreter : NodeVisitor
    {
        private Parser parser;

        private Dictionary<string, string> GLOBAL_SCOPE = new Dictionary<string,string>();

        public Interpreter(Parser parser_)
        {
            parser = parser_;
        }

        public string interpret()
        {
            AST tree = parser.parse();
            return tree.accept(this);
        }

        public string visit(Num node)
        {
            return node.value;
        }

        public string visit(BinOp node)
        {
            switch (node.op.type)
            {
                case tokens.PLUS:
                    return (Convert.ToString(Convert.ToInt32(node.left.accept(this)) + Convert.ToInt32(node.right.accept(this))));
                case tokens.MINUS:
                    return (Convert.ToString(Convert.ToInt32(node.left.accept(this)) + Convert.ToInt32(node.right.accept(this))));
                case tokens.MUL:
                    return (Convert.ToString(Convert.ToInt32(node.left.accept(this)) * Convert.ToInt32(node.right.accept(this))));
                case tokens.DIV:
                    return (Convert.ToString(Convert.ToInt32(node.left.accept(this)) / Convert.ToInt32(node.right.accept(this))));
            }
            return visit(node);
        }

        public string visit(UnaryOp node)
        {
            switch (node.op.type)
            {
                case tokens.PLUS:
                    return node.value.accept(this);
                case tokens.MINUS:
                    return Convert.ToString(-1 * Convert.ToInt32(node.value.accept(this)));
            }
            return visit(node);
        }

        public string visit(AST node)
        {
            throw new Exception(String.Format("No visit_{0} method", node.GetType()));
            return "";
        }

        public string visit(Compound node)
        {
            foreach(AST n in node.children)
            {
                n.accept(this);
            }
            return "";
        }

        public string visit(Assign node)
        {
            string name = node.left.value;
            GLOBAL_SCOPE[name] = node.right.accept(this);
            return GLOBAL_SCOPE[name];
        }

        public string visit(Var node)
        {
            string var_name = node.value;
            if (GLOBAL_SCOPE.ContainsKey(var_name))
                return GLOBAL_SCOPE[var_name];
            else
                throw new Exception("Error, variable not found: " + var_name);
            return "";
        }

        public string visit(NoOp node)
        {
            throw new NotImplementedException();
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
                Parser parser = new Parser(lexer);
                Interpreter intrp = new Interpreter(parser);
                Console.WriteLine(intrp.interpret());
            }
        }
    }
}
