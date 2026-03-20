namespace BakhmatovCalculatorLib.Models;

public sealed class CalculationHistoryItem
{
    public string Expression { get; set; } = string.Empty;
    public decimal Result { get; set; }
    public DateTime Timestamp { get; set; }

    public CalculationHistoryItem()
    {
    }

    public CalculationHistoryItem(string expression, decimal result, DateTime timestamp)
    {
        Expression = expression;
        Result = result;
        Timestamp = timestamp;
    }
}

