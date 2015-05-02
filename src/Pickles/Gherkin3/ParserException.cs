using System;
using System.Collections.Generic;
using System.Linq;
using Gherkin3.Ast;

namespace Gherkin3
{
    public abstract class ParserException : Exception
    {
        public Location Location { get; private set; }

        protected ParserException(string message) : base(message)
        {
        }

        protected ParserException(string message, Location location) : base(GetMessage(message, location))
        {
            if (location == null) throw new ArgumentNullException("location");

            this.Location = location;
        }

        private static string GetMessage(string message, Location location)
        {
            if (location == null) throw new ArgumentNullException("location");

            return string.Format("({0}:{1}): {2}", location.Line, location.Column, message);
        }
    }

    public class AstBuilderException : ParserException
    {
        public AstBuilderException(string message) : base(message)
        {
            if (message == null) throw new ArgumentNullException("message");
        }

        public AstBuilderException(string message, Location location) : base(message, location)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (location == null) throw new ArgumentNullException("location");
        }
    }

    public class NoSuchLanguageException : ParserException
    {
        public NoSuchLanguageException(string language, Location location) :
            base("Language not supported: " + language, location)
        {
        }
    }

    public abstract class TokenParserException : ParserException
    {
        protected TokenParserException(string message, Token receivedToken)
            : base(message, GetLocation(receivedToken))
        {
        }

        private static Location GetLocation(Token receivedToken)
        {
            return receivedToken.IsEOF || receivedToken.Location.Column > 1
                ? receivedToken.Location
                : new Location(receivedToken.Location.Line, receivedToken.Line.Indent + 1);
        }
    }

    public class UnexpectedTokenException : TokenParserException
    {
        public string StateComment { get; private set; }

        public Token ReceivedToken { get; private set; }
        public string[] ExpectedTokenTypes { get; private set; }

        public UnexpectedTokenException(Token receivedToken, string[] expectedTokenTypes, string stateComment)
            : base(GetMessage(receivedToken, expectedTokenTypes), receivedToken)
        {
            if (receivedToken == null) throw new ArgumentNullException("receivedToken");
            if (expectedTokenTypes == null) throw new ArgumentNullException("expectedTokenTypes");

            this.ReceivedToken = receivedToken;
            this.ExpectedTokenTypes = expectedTokenTypes;
            this.StateComment = stateComment;
        }

        private static string GetMessage(Token receivedToken, string[] expectedTokenTypes)
        {
            if (receivedToken == null) throw new ArgumentNullException("receivedToken");
            if (expectedTokenTypes == null) throw new ArgumentNullException("expectedTokenTypes");

            return string.Format("expected: {0}, got '{1}'",
                string.Join(", ", expectedTokenTypes),
                receivedToken.GetTokenValue().Trim());
        }
    }

    public class UnexpectedEOFException : TokenParserException
    {
        public string StateComment { get; private set; }
        public string[] ExpectedTokenTypes { get; private set; }
        public UnexpectedEOFException(Token receivedToken, string[] expectedTokenTypes, string stateComment)
            : base(GetMessage(expectedTokenTypes), receivedToken)
        {
            if (expectedTokenTypes == null) throw new ArgumentNullException("expectedTokenTypes");

            this.ExpectedTokenTypes = expectedTokenTypes;
            this.StateComment = stateComment;
        }

        private static string GetMessage(string[] expectedTokenTypes)
        {
            if (expectedTokenTypes == null) throw new ArgumentNullException("expectedTokenTypes");

            return string.Format("unexpected end of file, expected: {0}",
                string.Join(", ", expectedTokenTypes));
        }
    }

    public class CompositeParserException : ParserException
    {
        public IEnumerable<ParserException> Errors { get; private set; }

        public CompositeParserException(ParserException[] errors)
            : base(GetMessage(errors))
        {
            if (errors == null) throw new ArgumentNullException("errors");

            this.Errors = errors;
        }

        private static string GetMessage(ParserException[] errors)
        {
            if (errors == null) throw new ArgumentNullException("errors");

            return "Parser errors:" + Environment.NewLine + string.Join(Environment.NewLine, errors.Select(e => e.Message));
        }
    }
}