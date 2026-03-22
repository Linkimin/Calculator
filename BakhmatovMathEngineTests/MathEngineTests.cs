using BakhmatovMathEngine;
using BakhmatovMathEngine.Exceptions;
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
}