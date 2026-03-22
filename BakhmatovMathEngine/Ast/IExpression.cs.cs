namespace BakhmatovMathEngine.Ast;

public interface IExpression
{
    T Accept<T>(IExpressionVisitor<T> visitor);
}
