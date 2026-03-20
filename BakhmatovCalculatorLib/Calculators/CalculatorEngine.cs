using System.Collections.ObjectModel;
using BakhmatovCalculatorLib.Models;
using BakhmatovCalculatorLib.Services;

namespace BakhmatovCalculatorLib.Calculators;

public sealed class CalculatorEngine
{
    private const int MaxHistoryItems = 200;

    private readonly HistoryService _historyService;
    private readonly SettingsService _settingsService;

    private readonly List<CalculationHistoryItem> _history;
    private ThemeSettings _settings;

    public CalculatorEngine(string? historyFilePath = null, string? settingsFilePath = null)
    {
        _historyService = new HistoryService(historyFilePath);
        _settingsService = new SettingsService(settingsFilePath);

        _history = new List<CalculationHistoryItem>(_historyService.Load());
        _settings = _settingsService.Load();
    }

    public IReadOnlyList<CalculationHistoryItem> History => _history;
    public ThemeSettings Settings => _settings;

    public decimal Calculate(string expression)
    {
        var normalized = NormalizeExpression(expression);
        var result = ReversePolishNotation.Evaluate(normalized);
        AddHistory(normalized, result);
        return result;
    }

    // Used by UI for "preview" without polluting history.
    public decimal Evaluate(string expression)
    {
        var normalized = NormalizeExpression(expression);
        return ReversePolishNotation.Evaluate(normalized);
    }

    public IReadOnlyList<CalculationHistoryItem> GetHistory() => _history;

    public void ClearHistory()
    {
        _history.Clear();
        SaveAll();
    }

    public ThemeSettings GetThemeSettings() => _settings;

    public void UpdateThemeSettings(ThemeSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        SaveAll();
    }

    public void SaveAll()
    {
        _historyService.Save(_history);
        _settingsService.Save(_settings);
    }

    private void AddHistory(string expression, decimal result)
    {
        _history.Add(new CalculationHistoryItem(expression, result, DateTime.UtcNow));

        if (_history.Count > MaxHistoryItems)
            _history.RemoveRange(0, _history.Count - MaxHistoryItems);
    }

    private static string NormalizeExpression(string expression)
        => expression?.Trim() ?? throw new ArgumentNullException(nameof(expression));
}

