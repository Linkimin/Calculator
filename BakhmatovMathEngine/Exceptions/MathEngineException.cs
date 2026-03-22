namespace BakhmatovMathEngine.Exceptions;

public class MathEngineException : Exception
{
    public MathEngineException(string message) : base(message) { }
    public MathEngineException(string message, Exception inner) : base(message, inner) { }
}

public sealed class LexerException : MathEngineException
{
    public LexerException(string message) : base(message) { }
}

public sealed class ParserException : MathEngineException
{
    public ParserException(string message) : base(message) { }
}

public sealed class MathEvaluationException : MathEngineException
{
    public MathEvaluationException(string message) : base(message) { }
    public MathEvaluationException(string message, Exception inner) : base(message, inner) { }
}
