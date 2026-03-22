using BakhmatovMathEngine.Ast;
using BakhmatovMathEngine.Ast.Nodes;
using BakhmatovMathEngine.Evaluation;
using BakhmatovMathEngine.Values;

namespace BakhmatovMathEngine.Visitors;

public sealed class EvaluationVisitor : IExpressionVisitor<Value>
{
    private readonly EvaluationContext _context;

    public EvaluationVisitor(EvaluationContext context)
    {
        _context = context;
    }

    public Value VisitNumber(NumberNode node)
        => new NumberValue(node.Value);

    public Value VisitVariable(VariableNode node)
    {
        return node.Name.ToLower() switch
        {
            "pi" or "π" => new NumberValue((decimal)Math.PI),
            "e" => new NumberValue((decimal)Math.E),
            _ => _context.Get(node.Name)
        };
    }

    public Value VisitUnaryOp(UnaryOpNode node)
    {
        var operand = node.Operand.Accept(this);

        return (node.Operator, operand) switch
        {
            (UnaryOperator.Negate, NumberValue n) => new NumberValue(-n.Number),
            _ => throw new InvalidOperationException(
                $"Операция {node.Operator} не поддерживается для {operand.GetType().Name}.")
        };
    }

    public Value VisitBinaryOp(BinaryOpNode node)
    {
        var left = node.Left.Accept(this);
        var right = node.Right.Accept(this);

        return (node.Operator, left, right) switch
        {
            (BinaryOperator.Add, NumberValue l, NumberValue r) => new NumberValue(checked(l.Number + r.Number)),
            (BinaryOperator.Subtract, NumberValue l, NumberValue r) => new NumberValue(checked(l.Number - r.Number)),
            (BinaryOperator.Multiply, NumberValue l, NumberValue r) => new NumberValue(checked(l.Number * r.Number)),
            (BinaryOperator.Divide, NumberValue l, NumberValue r) => r.Number == 0m
                ? throw new DivideByZeroException("Деление на ноль.")
                : new NumberValue(checked(l.Number / r.Number)),
            (BinaryOperator.Power, NumberValue l, NumberValue r) => new NumberValue(PowDecimal(l.Number, r.Number)),
            _ => throw new InvalidOperationException(
                $"Операция {node.Operator} не поддерживается для {left.GetType().Name} и {right.GetType().Name}.")
        };
    }

    public Value VisitFunction(FunctionNode node)
    {
        var args = node.Arguments.Select(a => a.Accept(this)).ToList();

        if (args.Count == 1 && args[0] is NumberValue n)
        {
            double x = (double)n.Number;
            decimal result = node.Name.ToLower() switch
            {
                "sin" => (decimal)Math.Sin(x),
                "cos" => (decimal)Math.Cos(x),
                "tan" => (decimal)Math.Tan(x),
                "sqrt" => x < 0 ? throw new ArithmeticException("sqrt от отрицательного числа.") : (decimal)Math.Sqrt(x),
                "abs" => Math.Abs(n.Number),
                "ln" => x <= 0 ? throw new ArithmeticException("ln от неположительного числа.") : (decimal)Math.Log(x),
                "log" => x <= 0 ? throw new ArithmeticException("log от неположительного числа.") : (decimal)Math.Log10(x),
                "floor" => Math.Floor(n.Number),
                "ceil" => Math.Ceiling(n.Number),
                "round" => Math.Round(n.Number),
                _ => throw new NotSupportedException($"Функция '{node.Name}' не поддерживается.")
            };
            return new NumberValue(result);
        }

        throw new NotSupportedException($"Функция '{node.Name}' с такими аргументами не поддерживается.");
    }

    public Value VisitAssignment(AssignmentNode node)
    {
        var value = node.Value.Accept(this);
        _context.Set(node.Name, value);
        return value;
    }

    private static decimal PowDecimal(decimal baseVal, decimal exponent)
    {
        if (exponent == Math.Truncate(exponent) &&
            exponent >= int.MinValue && exponent <= int.MaxValue)
            return PowInt(baseVal, (int)exponent);

        return (decimal)Math.Pow((double)baseVal, (double)exponent);
    }

    private static decimal PowInt(decimal value, int exponent)
    {
        if (exponent == 0) return 1m;
        if (exponent < 0)
        {
            if (value == 0m) throw new DivideByZeroException("Деление на ноль в отрицательной степени.");
            long positiveExp = -(long)exponent;
            return 1m / PowInt(value, (int)positiveExp);
        }

        checked
        {
            decimal result = 1m;
            decimal factor = value;
            int e = exponent;
            while (e > 0)
            {
                if ((e & 1) == 1) result *= factor;
                e >>= 1;
                if (e > 0) factor *= factor;
            }
            return result;
        }
    }
}
