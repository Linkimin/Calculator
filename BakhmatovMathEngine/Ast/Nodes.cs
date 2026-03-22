using BakhmatovMathEngine.Ast;

namespace BakhmatovMathEngine.Ast.Nodes;

public sealed record NumberNode(decimal Value) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitNumber(this);
}

public sealed record VariableNode(string Name) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitVariable(this);
}

public enum BinaryOperator
{
    Add, Subtract, Multiply, Divide, Power
}

public sealed record BinaryOpNode(
    IExpression Left,
    BinaryOperator Operator,
    IExpression Right) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitBinaryOp(this);
}

public enum UnaryOperator
{
    Negate
}

public sealed record UnaryOpNode(
    UnaryOperator Operator,
    IExpression Operand) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitUnaryOp(this);
}

public sealed record FunctionNode(
    string Name,
    IReadOnlyList<IExpression> Arguments) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitFunction(this);
}

public sealed record AssignmentNode(string Name, IExpression Value) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitAssignment(this);
}
