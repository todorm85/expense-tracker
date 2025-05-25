using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class CategoryExpression
    {
        private readonly string _expression;
        private readonly List<Token> _tokens;
        private int _position;

        public CategoryExpression(string expression)
        {
            _expression = expression?.Trim() ?? string.Empty;
            _tokens = Tokenize(_expression);
            _position = 0;
        }

        public bool Evaluate(string[] categories)
        {
            if (string.IsNullOrWhiteSpace(_expression))
                return true;
            
            _position = 0;
            return EvaluateExpression(categories);
        }

        private bool EvaluateExpression(string[] categories)
        {
            var result = EvaluateTerm(categories);

            while (_position < _tokens.Count && (_tokens[_position].Type == TokenType.And || _tokens[_position].Type == TokenType.Or))
            {
                var op = _tokens[_position].Type;
                _position++;

                var nextTerm = EvaluateTerm(categories);

                if (op == TokenType.And)
                    result = result && nextTerm;
                else // op == TokenType.Or
                    result = result || nextTerm;
            }

            return result;
        }

        private bool EvaluateTerm(string[] categories)
        {
            if (_position >= _tokens.Count)
                return false;

            // Check for NOT operator
            bool negate = false;
            if (_tokens[_position].Type == TokenType.Not)
            {
                negate = true;
                _position++;
            }

            bool result;

            // Handle parenthesized expressions
            if (_tokens[_position].Type == TokenType.OpenParen)
            {
                _position++; // Skip open paren
                result = EvaluateExpression(categories);
                
                if (_position < _tokens.Count && _tokens[_position].Type == TokenType.CloseParen)
                    _position++; // Skip close paren
                else
                    throw new InvalidOperationException("Missing closing parenthesis");
            }
            // Handle category literals
            else if (_tokens[_position].Type == TokenType.Category)
            {
                var category = _tokens[_position].Value;
                _position++;
                
                // Check if the category is in the transaction's categories
                result = categories != null && categories.Any(c => 
                    string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                throw new InvalidOperationException($"Unexpected token: {_tokens[_position].Type}");
            }

            return negate ? !result : result;
        }

        private static List<Token> Tokenize(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return new List<Token>();

            var tokens = new List<Token>();
            var regex = new Regex(@"\(|\)|&&|\|\||!|[a-zA-Z0-9_-]+");
            
            foreach (Match match in regex.Matches(expression))
            {
                var value = match.Value;
                TokenType type;

                switch (value)
                {
                    case "(":
                        type = TokenType.OpenParen;
                        break;
                    case ")":
                        type = TokenType.CloseParen;
                        break;
                    case "&&":
                        type = TokenType.And;
                        break;
                    case "||":
                        type = TokenType.Or;
                        break;
                    case "!":
                        type = TokenType.Not;
                        break;
                    default:
                        type = TokenType.Category;
                        break;
                }

                tokens.Add(new Token(type, value));
            }

            return tokens;
        }

        private enum TokenType
        {
            OpenParen,
            CloseParen,
            And,
            Or,
            Not,
            Category
        }

        private class Token
        {
            public TokenType Type { get; }
            public string Value { get; }

            public Token(TokenType type, string value)
            {
                Type = type;
                Value = value;
            }
        }
    }
}
