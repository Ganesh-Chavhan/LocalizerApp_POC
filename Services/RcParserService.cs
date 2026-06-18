using System;
using System.Collections.Generic;
using Localizer_App.Models;

namespace Localizer_App.Services
{
    internal class ParserState
    {
        public bool InStringTable { get; set; }
        public int NestingLevel { get; set; }
        public Token? LastKeyToken { get; set; }
    }

    public class RcParserService
    {
        // Why: Service to extract ResourceString objects from string tables in an RC file.
        private readonly RcTokenizer _tokenizer = new RcTokenizer();

        public List<ResourceString> Parse(string fileContent)
        {
            // Why: Convert content to tokens and extract resource strings.
            var resourceStrings = new List<ResourceString>();
            if (string.IsNullOrEmpty(fileContent)) return resourceStrings;

            var tokens = _tokenizer.Tokenize(fileContent);
            ParseTokens(tokens, resourceStrings);
            return resourceStrings;
        }

        private void ParseTokens(List<Token> tokens, List<ResourceString> resourceStrings)
        {
            // Why: Loop through tokens to populate the target resource strings list.
            ParserState state = new ParserState();
            foreach (var token in tokens)
            {
                ProcessToken(token, state, resourceStrings);
            }
        }

        private void ProcessToken(Token token, ParserState state, List<ResourceString> resourceStrings)
        {
            // Why: Update the parser state or extract resource string based on token type.
            if (token.Type == TokenType.StringTable) ResetState(state);
            else if (token.Type == TokenType.Begin) HandleBegin(state);
            else if (token.Type == TokenType.End) HandleEnd(state);
            else HandleContentToken(token, state, resourceStrings);
        }

        private void ResetState(ParserState state)
        {
            // Why: Initialize or reset state when a new STRINGTABLE starts.
            state.InStringTable = true;
            state.NestingLevel = 0;
            state.LastKeyToken = null;
        }

        private void HandleBegin(ParserState state)
        {
            // Why: Increment nesting level if inside a string table.
            if (state.InStringTable) state.NestingLevel++;
        }

        private void HandleEnd(ParserState state)
        {
            // Why: Decrement nesting level and reset state if it exits the string table.
            if (!state.InStringTable) return;
            state.NestingLevel--;
            if (state.NestingLevel == 0) state.InStringTable = false;
        }

        private void HandleContentToken(Token token, ParserState state, List<ResourceString> resourceStrings)
        {
            // Why: Process identifiers as keys and string literals as values.
            if (!state.InStringTable || state.NestingLevel <= 0) return;
            bool isKey = token.Type == TokenType.Identifier || token.Type == TokenType.Number;
            if (isKey) state.LastKeyToken = token;
            else if (token.Type == TokenType.StringLiteral) TryAddLiteral(token, state, resourceStrings);
        }

        private void TryAddLiteral(Token token, ParserState state, List<ResourceString> resourceStrings)
        {
            // Why: Check if key exists and add literal to list.
            if (state.LastKeyToken == null) return;
            AddResourceString(token, state.LastKeyToken.Value, resourceStrings);
            state.LastKeyToken = null;
        }

        private void AddResourceString(Token token, string key, List<ResourceString> resourceStrings)
        {
            // Why: Parse and register the string literal into the target list.
            var text = UnescapeRcString(token.Value);
            resourceStrings.Add(new ResourceString
            {
                Key = key,
                Text = text,
                StartIndex = token.StartIndex,
                EndIndex = token.EndIndex
            });
        }

        public static string UnescapeRcString(string escapedText)
        {
            // Why: Remove enclosing quotes and restore double-quotes representation.
            if (string.IsNullOrEmpty(escapedText)) return string.Empty;
            bool isQuoted = escapedText.StartsWith("\"") && escapedText.EndsWith("\"") && escapedText.Length >= 2;
            if (isQuoted)
            {
                string content = escapedText.Substring(1, escapedText.Length - 2);
                return content.Replace("\"\"", "\"");
            }
            return escapedText;
        }
    }
}
