using BakhmatovMathEngine.Ast.Nodes;
using BakhmatovMathEngine.Evaluation;
using BakhmatovMathEngine.Exceptions;
using BakhmatovMathEngine.Parsing;
using BakhmatovMathEngine.Values;
using BakhmatovMathEngine.Visitors;

namespace BakhmatovMathEngineTests;

public sealed class ParserTests
{
    private static decimal Eval(string input, EvaluationContext? ctx = null)
    {
        var expr = Parser.Parse(input);
        var visitor = new EvaluationVisitor(ctx ?? EvaluationContext.Empty);
        var result = expr.Accept(visitor);
        return ((NumberValue)result).Number;
    }

    // ──────────────────────────────────────────────
    // Числа и базовые операции
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("2+3", 5)]
    [InlineData("10-4", 6)]
    [InlineData("7*8", 56)]
    [InlineData("20/5", 4)]
    [InlineData("2^10", 1024)]
    public void Parse_BasicOperations(string input, decimal expected)
    {
        Assert.Equal(expected, Eval(input));
    }

    [Theory]
    [InlineData("0.1+0.2", 0.3)]
    [InlineData("1,5+2,5", 4)]
    [InlineData("3.14*2", 6.28)]
    public void Parse_DecimalNumbers(string input, decimal expected)
    {
        Assert.Equal(expected, Eval(input));
    }

    // ──────────────────────────────────────────────
    // Приоритет и ассоциативность
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("2+3*4", 14)]
    [InlineData("(2+3)*4", 20)]
    [InlineData("2^3^2", 512)]  // правоассоциативный: 2^(3^2)=2^9
    [InlineData("10-2-3", 5)]    // левоассоциативный: (10-2)-3
    [InlineData("2*3+4*5", 26)]
    public void Parse_PrecedenceAndAssociativity(string input, decimal expected)
    {
        Assert.Equal(expected, Eval(input));
    }

    // ──────────────────────────────────────────────
    // Унарный минус
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("-5+2", -3)]
    [InlineData("3*-2", -6)]
    [InlineData("(-5)*(-2)", 10)]
    [InlineData("--5", 5)]
    public void Parse_UnaryMinus(string input, decimal expected)
    {
        Assert.Equal(expected, Eval(input));
    }

    // ──────────────────────────────────────────────
    // Переменные и константы
    // ──────────────────────────────────────────────

    [Fact]
    public void Parse_Variable_FromContext()
    {
        var ctx = EvaluationContext.Empty.Set("x", new NumberValue(5m));
        Assert.Equal(25m, Eval("x*x", ctx));
    }

    [Fact]
    public void Parse_Constant_Pi()
    {
        var result = Eval("pi");
        Assert.Equal((decimal)Math.PI, result);
    }

    [Fact]
    public void Parse_Constant_E()
    {
        var result = Eval("e");
        Assert.Equal((decimal)Math.E, result);
    }

    [Fact]
    public void Parse_Expression_WithVariable()
    {
        // x^2 + 2*x + 1 при x=3 = 16
        var ctx = EvaluationContext.Empty.Set("x", new NumberValue(3m));
        Assert.Equal(16m, Eval("x^2 + 2*x + 1", ctx));
    }

    // ──────────────────────────────────────────────
    // Функции
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("abs(-5)", 5)]
    [InlineData("abs(5)", 5)]
    [InlineData("floor(3.7)", 3)]
    [InlineData("ceil(3.2)", 4)]
    [InlineData("round(3.5)", 4)]
    public void Parse_ScalarFunctions(string input, decimal expected)
    {
        Assert.Equal(expected, Eval(input));
    }

    [Fact]
    public void Parse_Sqrt_Nine_IsThree()
    {
        Assert.Equal(3m, Eval("sqrt(9)"), 10);
    }

    [Fact]
    public void Parse_Sin_Pi_IsNearZero()
    {
        var result = Eval("sin(pi)");
        Assert.True(Math.Abs((double)result) < 1e-10);
    }

    [Fact]
    public void Parse_Cos_Zero_IsOne()
    {
        Assert.Equal(1m, Eval("cos(0)"), 10);
    }

    [Fact]
    public void Parse_NestedFunction()
    {
        // sqrt(abs(-16)) = 4
        Assert.Equal(4m, Eval("sqrt(abs(-16))"), 10);
    }

    [Fact]
    public void Parse_FunctionWithExpression()
    {
        // sin(pi/2) ≈ 1
        var result = Eval("sin(pi/2)");
        Assert.Equal(1m, Math.Round(result, 10));
    }

    // ──────────────────────────────────────────────
    // Присваивание
    // ──────────────────────────────────────────────

    [Fact]
    public void Parse_Assignment_StoresVariable()
    {
        var ctx = EvaluationContext.Empty;
        var expr = Parser.Parse("x = 2+3");
        var visitor = new EvaluationVisitor(ctx);
        expr.Accept(visitor);

        Assert.True(ctx.TryGet("x", out var value));
        Assert.Equal(5m, ((NumberValue)value!).Number);
    }

    [Fact]
    public void Parse_Assignment_CanBeUsedInNextExpression()
    {
        var ctx = EvaluationContext.Empty;
        var visitor = new EvaluationVisitor(ctx);

        Parser.Parse("x = 4").Accept(visitor);
        var result = (NumberValue)Parser.Parse("x * x").Accept(visitor);

        Assert.Equal(16m, result.Number);
    }

    // ──────────────────────────────────────────────
    // Негативные кейсы
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("(2+3")]     // незакрытая скобка
    [InlineData("2+")]       // нет правого операнда
    [InlineData("*3")]       // нет левого операнда
    [InlineData("")]         // пустое выражение
    public void Parse_InvalidExpression_ThrowsParserException(string input)
    {
        Assert.Throws<ParserException>(() => Parser.Parse(input));
    }

    [Fact]
    public void Parse_UnknownVariable_ThrowsOnEval()
    {
        Assert.Throws<InvalidOperationException>(() => Eval("y"));
    }

    [Fact]
    public void Parse_DivisionByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => Eval("8/0"));
    }
}
