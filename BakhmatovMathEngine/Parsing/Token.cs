namespace BakhmatovMathEngine.Parsing;

public enum TokenKind
{
    Number,         // 3.14
    Identifier,     // x, sin, pi
    Plus,           // +
    Minus,          // -
    Star,           // *
    Slash,          // /
    Caret,          // ^
    LParen,         // (
    RParen,         // )
    Comma,          // ,
    Equals,         // =  (для присваивания: x = 2+3)
    Eof             // конец входа
}

public sealed record Token(TokenKind Kind, string Text, int Position);
