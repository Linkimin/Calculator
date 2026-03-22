namespace BakhmatovMathEngine.Values;

public abstract record Value;

public sealed record NumberValue(decimal Number) : Value
{
    public override string ToString() => Number.ToString();
}

public sealed record MatrixValue(decimal[,] Data) : Value
{
    public int Rows => Data.GetLength(0);
    public int Cols => Data.GetLength(1);
    public override string ToString() => $"Matrix({Rows}×{Cols})";
}
