namespace BakhmatovCalculatorLib.Exceptions;

/// <summary>Базовое исключение для всех ошибок калькулятора.</summary>
public class CalculatorException : Exception
{
    public CalculatorException(string message) : base(message) { }
    public CalculatorException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Ошибка парсинга — неверный синтаксис выражения.</summary>
public sealed class ParsingException : CalculatorException
{
    public ParsingException(string message) : base(message) { }
}

/// <summary>Ошибка вычисления — деление на ноль, переполнение и т.д.</summary>
public sealed class EvaluationException : CalculatorException
{
    public EvaluationException(string message) : base(message) { }
    public EvaluationException(string message, Exception inner) : base(message, inner) { }
}
