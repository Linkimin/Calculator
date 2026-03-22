using BakhmatovMathEngine.Values;

namespace BakhmatovMathEngine.Evaluation;

public sealed class EvaluationContext
{
    private readonly Dictionary<string, Value> _variables = new();

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
}
