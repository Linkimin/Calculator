using BakhmatovMathEngine.Ast;
using BakhmatovMathEngine.Ast.Nodes;
using BakhmatovMathEngine.Evaluation;
using BakhmatovMathEngine.Values;
using BakhmatovMathEngine.Models;
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

        // Однаргументные числовые функции
        if (args.Count == 1 && args[0] is NumberValue n)
        {
            double x = (double)n.Number;
            double rad = _context.ToRadians(x); // угол с учётом AngleMode

            decimal result = node.Name.ToLower() switch
            {
                // Тригонометрия — используем rad
                "sin" => (decimal)Math.Sin(rad),
                "cos" => (decimal)Math.Cos(rad),
                "tan" => (decimal)Math.Tan(rad),

                // Обратная тригонометрия — результат конвертируем обратно
                "asin" => x < -1 || x > 1
                            ? throw new ArithmeticException("asin: аргумент вне [-1, 1].")
                            : (decimal)FromRadians(Math.Asin(x)),
                "acos" => x < -1 || x > 1
                            ? throw new ArithmeticException("acos: аргумент вне [-1, 1].")
                            : (decimal)FromRadians(Math.Acos(x)),
                "atan" => (decimal)FromRadians(Math.Atan(x)),

                // Гиперболические
                "sinh" => (decimal)Math.Sinh(x),
                "cosh" => (decimal)Math.Cosh(x),
                "tanh" => (decimal)Math.Tanh(x),

                // Прочие
                "sqrt" => x < 0
                            ? throw new ArithmeticException("sqrt: отрицательный аргумент.")
                            : (decimal)Math.Sqrt(x),
                "abs" => Math.Abs(n.Number),
                "ln" => x <= 0
                            ? throw new ArithmeticException("ln: аргумент должен быть > 0.")
                            : (decimal)Math.Log(x),
                "log" => x <= 0
                            ? throw new ArithmeticException("log: аргумент должен быть > 0.")
                            : (decimal)Math.Log10(x),
                "floor" => Math.Floor(n.Number),
                "ceil" => Math.Ceiling(n.Number),
                "round" => Math.Round(n.Number),
                "sign" => (decimal)Math.Sign(n.Number),
                "exp" => (decimal)Math.Exp(x),

                // Факториал
                "fact" => Factorial(n.Number),

                _ => throw new NotSupportedException($"Функция '{node.Name}' не поддерживается.")
            };
            return new NumberValue(result);
        }

        // Двухаргументные функции
        if (args.Count == 2 && args[0] is NumberValue a && args[1] is NumberValue b)
        {
            double x = (double)a.Number;
            double y = (double)b.Number;

            decimal result = node.Name.ToLower() switch
            {
                "log" => y <= 0 || y == 1
                            ? throw new ArithmeticException("log: неверное основание.")
                            : x <= 0
                                ? throw new ArithmeticException("log: аргумент должен быть > 0.")
                                : (decimal)(Math.Log(x) / Math.Log(y)),
                "pow" => (decimal)Math.Pow(x, y),
                "atan2" => (decimal)FromRadians(Math.Atan2(x, y)),
                _ => throw new NotSupportedException($"Функция '{node.Name}' с двумя аргументами не поддерживается.")
            };
            return new NumberValue(result);
        }

        throw new NotSupportedException($"Функция '{node.Name}' с {args.Count} аргументами не поддерживается.");
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

    /// <summary>Конвертирует результат из радиан в текущий режим.</summary>
    private double FromRadians(double radians) => _context.AngleMode switch
    {
        AngleMode.Rad => radians,
        AngleMode.Deg => radians * 180.0 / Math.PI,
        AngleMode.Grad => radians * 200.0 / Math.PI,
        _ => radians
    };

    private static decimal Factorial(decimal n)
    {
        if (n < 0)
            throw new ArithmeticException("fact: факториал отрицательного числа не определён.");
        if (n != Math.Truncate(n))
            throw new ArithmeticException("fact: факториал определён только для целых чисел.");
        if (n > 27)
            throw new OverflowException("fact: слишком большое число для decimal.");

        decimal result = 1m;
        for (int i = 2; i <= (int)n; i++)
            result *= i;
        return result;
    }
}
