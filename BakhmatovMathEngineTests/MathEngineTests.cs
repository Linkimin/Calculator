using BakhmatovMathEngine;
using BakhmatovMathEngine.Exceptions;
using BakhmatovMathEngine.Models;
using BakhmatovMathEngine.Values;

namespace BakhmatovMathEngineTests;

public sealed class MathEngineTests
{
    // ──────────────────────────────────────────────
    // Базовые вычисления
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("2+3", 5)]
    [InlineData("10-4", 6)]
    [InlineData("7*8", 56)]
    [InlineData("20/5", 4)]
    [InlineData("2^10", 1024)]
    [InlineData("sin(pi/2)", 1)]
    [InlineData("sqrt(9)", 3)]
    [InlineData("abs(-7)", 7)]
    public void Evaluate_BasicExpressions(string input, decimal expected)
    {
        var engine = new MathEngine();
        Assert.Equal(expected, Math.Round(engine.Evaluate(input), 10));
    }

    // ──────────────────────────────────────────────
    // Переменные через SetVariable
    // ──────────────────────────────────────────────

    [Fact]
    public void SetVariable_CanBeUsedInExpression()
    {
        var engine = new MathEngine();
        engine.SetVariable("x", 5m);
        Assert.Equal(25m, engine.Evaluate("x*x"));
    }

    [Fact]
    public void GetVariable_ReturnsSetValue()
    {
        var engine = new MathEngine();
        engine.SetVariable("radius", 3.14m);
        Assert.Equal(3.14m, engine.GetVariable("radius"));
    }

    [Fact]
    public void HasVariable_ReturnsTrueAfterSet()
    {
        var engine = new MathEngine();
        Assert.False(engine.HasVariable("x"));
        engine.SetVariable("x", 1m);
        Assert.True(engine.HasVariable("x"));
    }

    // ──────────────────────────────────────────────
    // Присваивание через выражение
    // ──────────────────────────────────────────────

    [Fact]
    public void Calculate_Assignment_StoresAndReturnsValue()
    {
        var engine = new MathEngine();
        var result = engine.Calculate("x = 2+3");

        Assert.IsType<NumberValue>(result);
        Assert.Equal(5m, ((NumberValue)result).Number);
        Assert.True(engine.HasVariable("x"));
        Assert.Equal(5m, engine.GetVariable("x"));
    }

    [Fact]
    public void Calculate_MultipleAssignments_CanUseEachOther()
    {
        var engine = new MathEngine();
        engine.Calculate("a = 3");
        engine.Calculate("b = 4");

        // c = sqrt(a^2 + b^2) = 5
        engine.Calculate("c = sqrt(a^2 + b^2)");
        Assert.Equal(5m, Math.Round(engine.GetVariable("c"), 10));
    }

    // ──────────────────────────────────────────────
    // Контекст сохраняется между вызовами
    // ──────────────────────────────────────────────

    [Fact]
    public void Engine_ContextPersistsBetweenCalls()
    {
        var engine = new MathEngine();
        engine.Evaluate("x = 10");
        engine.Evaluate("y = x * 2");
        Assert.Equal(20m, engine.GetVariable("y"));
    }

    // ──────────────────────────────────────────────
    // Негативные кейсы
    // ──────────────────────────────────────────────

    [Fact]
    public void Evaluate_EmptyString_Throws()
    {
        var engine = new MathEngine();
        Assert.Throws<MathEngineException>(() => engine.Evaluate(""));
    }

    [Fact]
    public void Evaluate_WhitespaceOnly_Throws()
    {
        var engine = new MathEngine();
        Assert.Throws<MathEngineException>(() => engine.Evaluate("   "));
    }

    [Fact]
    public void Evaluate_DivisionByZero_Throws()
    {
        var engine = new MathEngine();
        Assert.Throws<DivideByZeroException>(() => engine.Evaluate("1/0"));
    }

    [Fact]
    public void GetVariable_Undefined_Throws()
    {
        var engine = new MathEngine();
        Assert.Throws<MathEngineException>(() => engine.GetVariable("z"));
    }

    // ──────────────────────────────────────────────
    // AngleMode
    // ──────────────────────────────────────────────

    [Fact]
    public void AngleMode_Default_IsRadians()
    {
        var engine = new MathEngine();
        Assert.Equal(AngleMode.Rad, engine.AngleMode);
    }

    [Fact]
    public void Sin_InRadians_ReturnsCorrect()
    {
        var engine = new MathEngine();
        Assert.Equal(1m, Math.Round(engine.Evaluate("sin(pi/2)"), 10));
    }

    [Fact]
    public void Sin_InDegrees_ReturnsCorrect()
    {
        var engine = new MathEngine();
        engine.AngleMode = AngleMode.Deg;
        Assert.Equal(1m, Math.Round(engine.Evaluate("sin(90)"), 10));
    }

    [Fact]
    public void Cos_InDegrees_ReturnsCorrect()
    {
        var engine = new MathEngine();
        engine.AngleMode = AngleMode.Deg;
        Assert.Equal(1m, Math.Round(engine.Evaluate("cos(0)"), 10));
    }

    [Fact]
    public void Sin_InGrad_ReturnsCorrect()
    {
        var engine = new MathEngine();
        engine.AngleMode = AngleMode.Grad;
        // 100 grad = pi/2 rad → sin = 1
        Assert.Equal(1m, Math.Round(engine.Evaluate("sin(100)"), 10));
    }

    [Fact]
    public void Asin_InDegrees_Returns90()
    {
        var engine = new MathEngine();
        engine.AngleMode = AngleMode.Deg;
        Assert.Equal(90m, Math.Round(engine.Evaluate("asin(1)"), 10));
    }

    [Fact]
    public void Acos_InDegrees_Returns0()
    {
        var engine = new MathEngine();
        engine.AngleMode = AngleMode.Deg;
        Assert.Equal(0m, Math.Round(engine.Evaluate("acos(1)"), 10));
    }

    // ──────────────────────────────────────────────
    // Новые функции
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("sinh(0)", 0)]
    [InlineData("cosh(0)", 1)]
    [InlineData("tanh(0)", 0)]
    [InlineData("exp(0)", 1)]
    [InlineData("sign(-5)", -1)]
    [InlineData("sign(5)", 1)]
    [InlineData("sign(0)", 0)]
    public void NewFunctions_ReturnCorrectResults(string input, decimal expected)
    {
        var engine = new MathEngine();
        Assert.Equal(expected, Math.Round(engine.Evaluate(input), 10));
    }

    [Theory]
    [InlineData("fact(0)", 1)]
    [InlineData("fact(1)", 1)]
    [InlineData("fact(5)", 120)]
    [InlineData("fact(10)", 3628800)]
    public void Factorial_ReturnsCorrectResults(string input, decimal expected)
    {
        var engine = new MathEngine();
        Assert.Equal(expected, engine.Evaluate(input));
    }

    [Fact]
    public void Factorial_Negative_Throws()
    {
        var engine = new MathEngine();
        Assert.Throws<ArithmeticException>(() => engine.Evaluate("fact(-1)"));
    }

    [Fact]
    public void Factorial_Fractional_Throws()
    {
        var engine = new MathEngine();
        Assert.Throws<ArithmeticException>(() => engine.Evaluate("fact(1.5)"));
    }

    [Fact]
    public void Log_WithBase_ReturnsCorrect()
    {
        var engine = new MathEngine();
        // log(8, 2) = 3
        Assert.Equal(3m, Math.Round(engine.Evaluate("log(8, 2)"), 10));
    }

    [Fact]
    public void Log_Base10_ReturnsCorrect()
    {
        var engine = new MathEngine();
        Assert.Equal(2m, Math.Round(engine.Evaluate("log(100)"), 10));
    }
}