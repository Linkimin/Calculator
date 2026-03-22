using BakhmatovMathEngine.Ast.Nodes;

namespace BakhmatovMathEngine.Ast;

public interface IExpressionVisitor<out T>
{
    T VisitNumber(NumberNode node);
    T VisitVariable(VariableNode node);
    T VisitBinaryOp(BinaryOpNode node);
    T VisitUnaryOp(UnaryOpNode node);
    T VisitFunction(FunctionNode node);
    T VisitAssignment(AssignmentNode node);
}
