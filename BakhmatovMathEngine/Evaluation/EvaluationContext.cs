using BakhmatovMathEngine.Values;
using BakhmatovMathEngine.Models;

namespace BakhmatovMathEngine.Evaluation;

public sealed class EvaluationContext
{
    private readonly Dictionary<string, Value> _variables = new();

    public AngleMode AngleMode { get; set; } = AngleMode.Rad;

    public static EvaluationContext Empty => new();

    public EvaluationContext Set(string name, Value value)
    {
        _variables[name] = value;
        return this;
    }

    public Value Get(string name)
    {
        if (_variables.TryGetValue(name, out var value))
            return value;

        throw new InvalidOperationException($"Переменная '{name}' не определена.");
    }

    public bool TryGet(string name, out Value? value)
        => _variables.TryGetValue(name, out value);

    /// <summary>Конвертирует угол из текущего режима в радианы.</summary>
    public double ToRadians(double angle) => AngleMode switch
    {
        AngleMode.Rad => angle,
        AngleMode.Deg => angle * Math.PI / 180.0,
        AngleMode.Grad => angle * Math.PI / 200.0,
        _ => angle
    };
}
