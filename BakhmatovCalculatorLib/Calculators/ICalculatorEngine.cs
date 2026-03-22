namespace BakhmatovCalculatorLib.Calculators;

/// <summary>
/// Контракт вычислительного движка.
/// Не включает историю и настройки — это отдельные ответственности.
/// </summary>
public interface ICalculatorEngine
{
    /// <summary>Вычисляет выражение и сохраняет результат в историю.</summary>
    decimal Calculate(string expression);

    /// <summary>Вычисляет выражение без записи в историю (предпросмотр).</summary>
    decimal Evaluate(string expression);
}
