using BakhmatovCalculatorLib.Models;

namespace BakhmatovCalculatorLib.Services;

public interface IHistoryService
{
    IReadOnlyList<CalculationHistoryItem> Load();
    void Save(IEnumerable<CalculationHistoryItem> items);
}
