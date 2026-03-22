using System.Globalization;

namespace BakhmatovMathEngine.Parsing;

public sealed class Lexer
{
    private readonly string _input;
    private int _pos;

    public Lexer(string input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
    }

    public IReadOnlyList<Token> Tokenize()
    {
        var tokens = new List<Token>();
        _pos = 0;

        while (_pos < _input.Length)
        {
            SkipWhitespace();

            if (_pos >= _input.Length)
                break;

            var token = ReadNext();
            tokens.Add(token);
        }

        tokens.Add(new Token(TokenKind.Eof, "", _pos));
        return tokens;
    }

    private Token ReadNext()
    {
        int start = _pos;
        char c = _input[_pos];

        // Числа
        if (char.IsDigit(c) || (c == '.'))
            return ReadNumber(start);

        // Идентификаторы (переменные, функции, константы)
        if (char.IsLetter(c) || c == '_' || c == 'π')
            return ReadIdentifier(start);

        // Операторы и скобки
        _pos++;
        return c switch
        {
            '+' => new Token(TokenKind.Plus, "+", start),
            '-' => new Token(TokenKind.Minus, "-", start),
            '*' => new Token(TokenKind.Star, "*", start),
            '/' => new Token(TokenKind.Slash, "/", start),
            '^' => new Token(TokenKind.Caret, "^", start),
            '(' => new Token(TokenKind.LParen, "(", start),
            ')' => new Token(TokenKind.RParen, ")", start),
            ',' => new Token(TokenKind.Comma, ",", start),
            '=' => new Token(TokenKind.Equals, "=", start),
            _ => throw new BakhmatovMathEngine.Exceptions.LexerException(
                       $"Неизвестный символ '{c}' на позиции {start}.")
        };
    }

    private Token ReadNumber(int start)
    {
        bool seenSeparator = false;

        while (_pos < _input.Length)
        {
            char ch = _input[_pos];

            if (char.IsDigit(ch))
            {
                _pos++;
                continue;
            }

            if ((ch == '.' || ch == ',') && !seenSeparator)
            {
                // Читаем запятую как разделитель только если следующий символ — цифра
                if (ch == ',' && (_pos + 1 >= _input.Length || !char.IsDigit(_input[_pos + 1])))
                    break;

                seenSeparator = true;
                _pos++;
                continue;
            }
            break;
        }

        var text = _input[start.._pos].Replace(',', '.');

        if (!decimal.TryParse(text, NumberStyles.Number,
                CultureInfo.InvariantCulture, out _))
            throw new BakhmatovMathEngine.Exceptions.LexerException(
                $"Неверный формат числа '{text}' на позиции {start}.");

        return new Token(TokenKind.Number, text, start);
    }

    private Token ReadIdentifier(int start)
    {
        while (_pos < _input.Length)
        {
            char ch = _input[_pos];
            if (char.IsLetterOrDigit(ch) || ch == '_' || ch == 'π')
                _pos++;
            else
                break;
        }

        return new Token(TokenKind.Identifier, _input[start.._pos], start);
    }

    private void SkipWhitespace()
    {
        while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
            _pos++;
    }
}