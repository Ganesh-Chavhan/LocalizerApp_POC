using System;
using System.Collections.Generic;

namespace Localizer_App.Services
{
    public enum TokenType
    {
        StringTable,
        Begin,
        End,
        Identifier,
        Number,
        StringLiteral,
        Other
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class RcTokenizer
    {
        // Why: Helper class to tokenize a C++ resource script (.rc) file.
        
        public List<Token> Tokenize(string text)
        {
            // Why: Loop through all characters to split the file into tokens.
            List<Token> tokens = new List<Token>();
            int index = 0;
            while (index < text.Length)
            {
                index = ParseNext(text, index, tokens);
            }
            return tokens;
        }

        private int ParseNext(string text, int index, List<Token> tokens)
        {
            // Why: Determine the type of character at index and process it.
            char character = text[index];
            if (char.IsWhiteSpace(character)) return index + 1;
            if (IsComment(text, index)) return SkipComment(text, index);
            if (character == '#') return SkipDirective(text, index);
            return ReadContentToken(text, index, tokens);
        }

        private bool IsComment(string text, int index)
        {
            // Why: Check if the character sequence starting at index is a comment.
            bool isSlash = text[index] == '/';
            bool hasNext = index + 1 < text.Length;
            char nextChar = hasNext ? text[index + 1] : '\0';
            return isSlash && (nextChar == '/' || nextChar == '*');
        }

        private int SkipComment(string text, int index)
        {
            // Why: Delegate to single-line or multi-line comment skipping.
            if (text[index + 1] == '/')
            {
                return SkipSingleLineComment(text, index + 2);
            }
            return SkipBlockComment(text, index + 2);
        }

        private int SkipSingleLineComment(string text, int index)
        {
            // Why: Skip characters until the end of the line.
            int current = index;
            while (current < text.Length && text[current] != '\n' && text[current] != '\r')
            {
                current++;
            }
            return current;
        }

        private int SkipBlockComment(string text, int index)
        {
            // Why: Skip characters until the closing comment tag "*/" is found.
            int current = index;
            while (current < text.Length)
            {
                bool isStar = text[current] == '*';
                bool nextIsSlash = current + 1 < text.Length && text[current + 1] == '/';
                if (isStar && nextIsSlash) return current + 2;
                current++;
            }
            return current;
        }

        private int SkipDirective(string text, int index)
        {
            // Why: Skip preprocessor directives until the end of the line.
            return SkipSingleLineComment(text, index + 1);
        }

        private int ReadContentToken(string text, int index, List<Token> tokens)
        {
            // Why: Branch to string, identifier, number, brackets, or other symbols.
            char character = text[index];
            if (character == '"') return ReadStringLiteral(text, index, tokens);
            if (char.IsLetter(character) || character == '_') return ReadIdentifier(text, index, tokens);
            if (char.IsDigit(character)) return ReadNumber(text, index, tokens);
            return ReadSymbol(text, index, tokens);
        }

        private int ReadStringLiteral(string text, int index, List<Token> tokens)
        {
            // Why: Find the matching end quote while ignoring escaped double-quotes.
            int current = index + 1;
            while (current < text.Length && text[current] != '"')
            {
                current += IsEscapedQuote(text, current) ? 2 : 1;
            }
            return AddStringToken(text, index, current, tokens);
        }

        private bool IsEscapedQuote(string text, int current)
        {
            // Why: Determine if the quote or backslash sequence escapes a quote.
            bool doubleQuotes = text[current] == '"' && current + 1 < text.Length && text[current + 1] == '"';
            bool backslashQuote = text[current] == '\\' && current + 1 < text.Length;
            return doubleQuotes || backslashQuote;
        }

        private int AddStringToken(string text, int start, int end, List<Token> tokens)
        {
            // Why: Add a StringLiteral token to the collection and return next index.
            int limit = Math.Min(end - start + 1, text.Length - start);
            string val = text.Substring(start, limit);
            tokens.Add(new Token { Type = TokenType.StringLiteral, Value = val, StartIndex = start, EndIndex = end });
            return end + 1;
        }

        private int ReadIdentifier(string text, int index, List<Token> tokens)
        {
            // Why: Read characters of a word and classify it as a keyword or variable.
            int current = index;
            while (current < text.Length && (char.IsLetterOrDigit(text[current]) || text[current] == '_'))
            {
                current++;
            }
            string val = text.Substring(index, current - index);
            TokenType type = GetKeywordType(val);
            tokens.Add(new Token { Type = type, Value = val, StartIndex = index, EndIndex = current - 1 });
            return current;
        }

        private TokenType GetKeywordType(string val)
        {
            // Why: Match keywords (STRINGTABLE, BEGIN, END) case-insensitively.
            if (val.Equals("STRINGTABLE", StringComparison.OrdinalIgnoreCase)) return TokenType.StringTable;
            if (val.Equals("BEGIN", StringComparison.OrdinalIgnoreCase)) return TokenType.Begin;
            if (val.Equals("END", StringComparison.OrdinalIgnoreCase)) return TokenType.End;
            return TokenType.Identifier;
        }

        private int ReadNumber(string text, int index, List<Token> tokens)
        {
            // Why: Read digit characters for resource IDs.
            int current = index;
            while (current < text.Length && char.IsDigit(text[current]))
            {
                current++;
            }
            string val = text.Substring(index, current - index);
            tokens.Add(new Token { Type = TokenType.Number, Value = val, StartIndex = index, EndIndex = current - 1 });
            return current;
        }

        private int ReadSymbol(string text, int index, List<Token> tokens)
        {
            // Why: Add special characters (like braces) to tokens list.
            char character = text[index];
            TokenType type = GetSymbolType(character);
            tokens.Add(new Token { Type = type, Value = character.ToString(), StartIndex = index, EndIndex = index });
            return index + 1;
        }

        private TokenType GetSymbolType(char character)
        {
            // Why: Classify single characters as BEGIN/END blocks or other.
            if (character == '{') return TokenType.Begin;
            if (character == '}') return TokenType.End;
            return TokenType.Other;
        }
    }
}
