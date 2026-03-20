using BakhmatovCalculatorLib.Calculators;

namespace BakhmatovCalculatorTests;

public sealed class CalculatorEngineTests
{
    private static CalculatorEngine CreateEngine()
    {
        var root = Path.Combine(Path.GetTempPath(), "BakhmatovCalculatorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return new CalculatorEngine(
            historyFilePath: Path.Combine(root, "history.json"),
            settingsFilePath: Path.Combine(root, "settings.json"));
    }

    [Fact]
    public void Calculate_Addition_ReturnsCorrectResult()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("2+3");
        Assert.Equal(5m, result);
    }

    [Fact]
    public void Calculate_Subtraction_ReturnsCorrectResult()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("10-4");
        Assert.Equal(6m, result);
    }

    [Fact]
    public void Calculate_Multiplication_ReturnsCorrectResult()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("7*8");
        Assert.Equal(56m, result);
    }

    [Fact]
    public void Calculate_Division_ReturnsCorrectResult()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("20/5");
        Assert.Equal(4m, result);
    }

    [Fact]
    public void Calculate_Power_ReturnsCorrectResult()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("2^10");
        Assert.Equal(1024m, result);
    }

    [Fact]
    public void Calculate_OperatorPrecedence_MultiplicationBeforeAddition()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("2+3*4");
        Assert.Equal(14m, result);
    }

    [Fact]
    public void Calculate_Parentheses_AffectPrecedence()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("(2+3)*4");
        Assert.Equal(20m, result);
    }

    [Fact]
    public void Calculate_RightAssociativePower_IsHandled()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("2^3^2");
        Assert.Equal(512m, result);
    }

    [Fact]
    public void Calculate_UnaryMinus_AtStart_IsHandled()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("-5+2");
        Assert.Equal(-3m, result);
    }

    [Fact]
    public void Calculate_UnaryMinus_AfterOperator_IsHandled()
    {
        var engine = CreateEngine();
        var result = engine.Calculate("3*-2");
        Assert.Equal(-6m, result);
    }

    [Fact]
    public void Calculate_DivisionByZero_Throws()
    {
        var engine = CreateEngine();
        Assert.Throws<DivideByZeroException>(() => engine.Calculate("8/0"));
    }

    [Fact]
    public void Calculate_Overflow_Throws()
    {
        var engine = CreateEngine();
        Assert.Throws<OverflowException>(() => engine.Calculate("79228162514264337593543950335*2"));
    }
}

