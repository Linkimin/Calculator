using BakhmatovMathEngine.Evaluation;
using BakhmatovMathEngine.Exceptions;
using BakhmatovMathEngine.Parsing;
using BakhmatovMathEngine.Values;
using BakhmatovMathEngine.Visitors;
using BakhmatovMathEngine.Models;

namespace BakhmatovMathEngine;

public sealed class MathEngine
{
    private readonly EvaluationContext _context;

    public MathEngine() : this(EvaluationContext.Empty) { }

    public MathEngine(EvaluationContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Вычисляет выражение. Поддерживает присваивание: x = 2+3.
    /// </summary>
    public Value Calculate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new MathEngineException("Выражение не может быть пустым.");

        var expr = Parser.Parse(expression.Trim());
        var visitor = new EvaluationVisitor(_context);
        return expr.Accept(visitor);
    }

    /// <summary>
    /// Вычисляет выражение и возвращает decimal.
    /// Бросает исключение если результат не число.
    /// </summary>
    public decimal Evaluate(string expression)
    {
        var result = Calculate(expression);

        return result is NumberValue n
            ? n.Number
            : throw new MathEngineException(
                $"Ожидалось число, получен {result.GetType().Name}.");
    }

    /// <summary>
    /// Устанавливает переменную в контексте.
    /// </summary>
    public void SetVariable(string name, decimal value)
        => _context.Set(name, new NumberValue(value));

    /// <summary>
    /// Получает значение переменной из контекста.
    /// </summary>
    public decimal GetVariable(string name)
    {
        if (_context.TryGet(name, out var value) && value is NumberValue n)
            return n.Number;

        throw new MathEngineException($"Переменная '{name}' не определена или не является числом.");
    }

    /// <summary>
    /// Проверяет существование переменной в контексте.
    /// </summary>
    public bool HasVariable(string name)
        => _context.TryGet(name, out _);

    public AngleMode AngleMode
    {
        get => _context.AngleMode;
        set => _context.AngleMode = value;
    }
}
