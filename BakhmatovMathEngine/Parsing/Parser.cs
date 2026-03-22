using BakhmatovMathEngine.Ast;
using BakhmatovMathEngine.Ast.Nodes;
using BakhmatovMathEngine.Exceptions;

namespace BakhmatovMathEngine.Parsing;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _pos;

    private Token Current => _tokens[_pos];
    private Token Peek(int offset = 1) => _tokens[Math.Min(_pos + offset, _tokens.Count - 1)];

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    public static IExpression Parse(string input)
    {
        var tokens = new Lexer(input).Tokenize();
        return new Parser(tokens).ParseExpression();
    }

    public IExpression ParseExpression()
    {
        var expr = ParseAssignment();

        if (Current.Kind != TokenKind.Eof)
            throw new ParserException(
                $"Неожиданный токен '{Current.Text}' на позиции {Current.Position}.");

        return expr;
    }

    // ── Присваивание: x = expr ──────────────────────────────
    private IExpression ParseAssignment()
    {
        // Lookahead: identifier followed by '='
        if (Current.Kind == TokenKind.Identifier &&
            Peek().Kind == TokenKind.Equals)
        {
            var name = Current.Text;
            Advance(); // skip identifier
            Advance(); // skip '='
            var value = ParseAddSub();
            return new AssignmentNode(name, value);
        }

        return ParseAddSub();
    }

    // ── Сложение / вычитание ────────────────────────────────
    private IExpression ParseAddSub()
    {
        var left = ParseMulDiv();

        while (Current.Kind is TokenKind.Plus or TokenKind.Minus)
        {
            var op = Current.Kind == TokenKind.Plus
                ? BinaryOperator.Add
                : BinaryOperator.Subtract;
            Advance();
            var right = ParseMulDiv();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    // ── Умножение / деление ─────────────────────────────────
    private IExpression ParseMulDiv()
    {
        var left = ParsePower();

        while (Current.Kind is TokenKind.Star or TokenKind.Slash)
        {
            var op = Current.Kind == TokenKind.Star
                ? BinaryOperator.Multiply
                : BinaryOperator.Divide;
            Advance();
            var right = ParsePower();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    // ── Степень (правоассоциативная) ────────────────────────
    private IExpression ParsePower()
    {
        var left = ParseUnary();

        if (Current.Kind == TokenKind.Caret)
        {
            Advance();
            var right = ParsePower(); // правая рекурсия = правая ассоциативность
            return new BinaryOpNode(left, BinaryOperator.Power, right);
        }

        return left;
    }

    // ── Унарный минус ───────────────────────────────────────
    private IExpression ParseUnary()
    {
        if (Current.Kind == TokenKind.Minus)
        {
            Advance();
            var operand = ParseUnary();
            return new UnaryOpNode(UnaryOperator.Negate, operand);
        }

        return ParsePrimary();
    }

    // ── Первичные выражения ─────────────────────────────────
    private IExpression ParsePrimary()
    {
        // Число
        if (Current.Kind == TokenKind.Number)
        {
            var value = decimal.Parse(Current.Text,
                System.Globalization.CultureInfo.InvariantCulture);
            Advance();
            return new NumberNode(value);
        }

        // Идентификатор: переменная или вызов функции
        if (Current.Kind == TokenKind.Identifier)
        {
            var name = Current.Text;
            Advance();

            // Вызов функции: name(arg1, arg2, ...)
            if (Current.Kind == TokenKind.LParen)
            {
                Advance(); // skip '('
                var args = new List<IExpression>();

                if (Current.Kind != TokenKind.RParen)
                {
                    args.Add(ParseAddSub());
                    while (Current.Kind == TokenKind.Comma)
                    {
                        Advance(); // skip ','
                        args.Add(ParseAddSub());
                    }
                }

                Expect(TokenKind.RParen);
                return new FunctionNode(name, args);
            }

            return new VariableNode(name);
        }

        // Скобки: (expr)
        if (Current.Kind == TokenKind.LParen)
        {
            Advance(); // skip '('
            var expr = ParseAddSub();
            Expect(TokenKind.RParen);
            return expr;
        }

        throw new ParserException(
            $"Неожиданный токен '{Current.Text}' на позиции {Current.Position}.");
    }

    private Token Advance()
    {
        var current = _tokens[_pos];
        if (_pos < _tokens.Count - 1)
            _pos++;
        return current;
    }

    private void Expect(TokenKind kind)
    {
        if (Current.Kind != kind)
            throw new ParserException(
                $"Ожидался '{kind}', получен '{Current.Text}' на позиции {Current.Position}.");
        Advance();
    }
}
