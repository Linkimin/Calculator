using BakhmatovMathEngine.Exceptions;
using BakhmatovMathEngine.Parsing;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;

namespace BakhmatovMathEngineTests;

public sealed class LexerTests
{
    private static IReadOnlyList<Token> Lex(string input) =>
        new Lexer(input).Tokenize();

    private static Token[] WithoutEof(IReadOnlyList<Token> tokens) =>
        tokens.Where(t => t.Kind != TokenKind.Eof).ToArray();

    // ──────────────────────────────────────────────
    // Числа
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("42", "42")]
    [InlineData("3.14", "3.14")]
    [InlineData("3,14", "3.14")]  // запятая нормализуется в точку
    [InlineData("0.5", "0.5")]
    [InlineData("1000", "1000")]
    public void Number_TokenizedCorrectly(string input, string expectedText)
    {
        var tokens = WithoutEof(Lex(input));
        Assert.Single(tokens);
        Assert.Equal(TokenKind.Number, tokens[0].Kind);
        Assert.Equal(expectedText, tokens[0].Text);
    }

    // ──────────────────────────────────────────────
    // Идентификаторы
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("x")]
    [InlineData("pi")]
    [InlineData("sin")]
    [InlineData("myVar")]
    [InlineData("π")]
    public void Identifier_TokenizedCorrectly(string input)
    {
        var tokens = WithoutEof(Lex(input));
        Assert.Single(tokens);
        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal(input, tokens[0].Text);
    }

    // ──────────────────────────────────────────────
    // Операторы
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("+", TokenKind.Plus)]
    [InlineData("-", TokenKind.Minus)]
    [InlineData("*", TokenKind.Star)]
    [InlineData("/", TokenKind.Slash)]
    [InlineData("^", TokenKind.Caret)]
    [InlineData("(", TokenKind.LParen)]
    [InlineData(")", TokenKind.RParen)]
    [InlineData(",", TokenKind.Comma)]
    [InlineData("=", TokenKind.Equals)]
    public void Operator_TokenizedCorrectly(string input, TokenKind expected)
    {
        var tokens = WithoutEof(Lex(input));
        Assert.Single(tokens);
        Assert.Equal(expected, tokens[0].Kind);
    }

    // ──────────────────────────────────────────────
    // Составные выражения
    // ──────────────────────────────────────────────

    [Fact]
    public void Expression_2Plus3_ProducesThreeTokens()
    {
        var tokens = WithoutEof(Lex("2+3"));
        Assert.Equal(3, tokens.Length);
        Assert.Equal(TokenKind.Number, tokens[0].Kind);
        Assert.Equal(TokenKind.Plus, tokens[1].Kind);
        Assert.Equal(TokenKind.Number, tokens[2].Kind);
    }

    [Fact]
    public void Expression_WithSpaces_IgnoresWhitespace()
    {
        var tokens = WithoutEof(Lex("  2  +  3  "));
        Assert.Equal(3, tokens.Length);
    }

    [Fact]
    public void Expression_FunctionCall_TokenizedCorrectly()
    {
        // sin(pi/2)
        var tokens = WithoutEof(Lex("sin(pi/2)"));
        Assert.Equal(6, tokens.Length); // sin ( pi / 2 )
        Assert.Equal(TokenKind.Identifier, tokens[0].Kind); // sin
        Assert.Equal(TokenKind.LParen, tokens[1].Kind); // (
        Assert.Equal(TokenKind.Identifier, tokens[2].Kind); // pi
        Assert.Equal(TokenKind.Slash, tokens[3].Kind); // /
        Assert.Equal(TokenKind.Number, tokens[4].Kind); // 2
        Assert.Equal(TokenKind.RParen, tokens[5].Kind); // )
    }

    [Fact]
    public void Expression_Assignment_TokenizedCorrectly()
    {
        // x = 2+3
        var tokens = WithoutEof(Lex("x = 2+3"));
        Assert.Equal(5, tokens.Length);
        Assert.Equal(TokenKind.Identifier, tokens[0].Kind); // x
        Assert.Equal(TokenKind.Equals, tokens[1].Kind); // =
        Assert.Equal(TokenKind.Number, tokens[2].Kind); // 2
        Assert.Equal(TokenKind.Plus, tokens[3].Kind); // +
        Assert.Equal(TokenKind.Number, tokens[4].Kind); // 3
    }

    [Fact]
    public void Expression_NestedParens_TokenizedCorrectly()
    {
        // (2+3)*4
        var tokens = WithoutEof(Lex("(2+3)*4"));
        Assert.Equal(7, tokens.Length);
        Assert.Equal(TokenKind.LParen, tokens[0].Kind);
        Assert.Equal(TokenKind.RParen, tokens[4].Kind);
        Assert.Equal(TokenKind.Star, tokens[5].Kind);
    }

    [Fact]
    public void Expression_MultiArgFunction_TokenizedCorrectly()
    {
        // log(100, 10) — используем пробел после запятой чтобы избежать
        // конфликта с десятичным разделителем
        var tokens = WithoutEof(Lex("log(100, 10)"));
        Assert.Equal(6, tokens.Length);
        Assert.Equal(TokenKind.Identifier, tokens[0].Kind); // log
        Assert.Equal(TokenKind.LParen, tokens[1].Kind); // (
        Assert.Equal(TokenKind.Number, tokens[2].Kind); // 100
        Assert.Equal(TokenKind.Comma, tokens[3].Kind); // ,
        Assert.Equal(TokenKind.Number, tokens[4].Kind); // 10
        Assert.Equal(TokenKind.RParen, tokens[5].Kind); // )
    }

    // ──────────────────────────────────────────────
    // Позиции токенов
    // ──────────────────────────────────────────────

    [Fact]
    public void Tokens_HaveCorrectPositions()
    {
        var tokens = Lex("2+3");
        Assert.Equal(0, tokens[0].Position); // 2
        Assert.Equal(1, tokens[1].Position); // +
        Assert.Equal(2, tokens[2].Position); // 3
    }

    // ──────────────────────────────────────────────
    // EOF
    // ──────────────────────────────────────────────

    [Fact]
    public void EmptyString_ReturnsOnlyEof()
    {
        var tokens = Lex("");
        Assert.Single(tokens);
        Assert.Equal(TokenKind.Eof, tokens[0].Kind);
    }

    [Fact]
    public void Whitespace_ReturnsOnlyEof()
    {
        var tokens = Lex("   ");
        Assert.Single(tokens);
        Assert.Equal(TokenKind.Eof, tokens[0].Kind);
    }

    // ──────────────────────────────────────────────
    // Негативные кейсы
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("2@3")]
    [InlineData("2#3")]
    [InlineData("2$3")]
    public void UnknownSymbol_ThrowsLexerException(string input)
    {
        Assert.Throws<LexerException>(() => new Lexer(input).Tokenize());
    }
}
