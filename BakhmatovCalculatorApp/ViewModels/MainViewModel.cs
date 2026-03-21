using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BakhmatovCalculatorLib.Calculators;
using BakhmatovCalculatorLib.Models;
using BakhmatovCalculatorLib.Services;
using BakhmatovCalculatorApp.Infrastructure;

namespace BakhmatovCalculatorApp.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private enum UiLanguage { Ru, En }

    private readonly CalculatorEngine _engine;
    private readonly RelayCommand _evaluateCommand;
    private UiLanguage _currentLanguage;

    private string _expression = string.Empty;
    private string _resultText = string.Empty;

    private string _pageBackground = "#000000";
    private string _panelBackground = "#000000";
    private string _keyBackground = "#000000";
    private string _hoverKeyBackground = "#000000";
    private string _pressedKeyBackground = "#000000";
    private string _keyBorderBrush = "#000000";
    private string _keyForeground = "#000000";
    private string _accentKeyBackground = "#1E90FF";
    private string _hoverAccentKeyBackground = "#1E90FF";
    private string _pressedAccentKeyBackground = "#1E90FF";
    private string _accentKeyForeground = "#FFFFFF";
    private string _secondaryText = "#888888";

    private string _windowTitle = string.Empty;
    private string _themeButtonText = string.Empty;
    private string _languageButtonText = string.Empty;
    private string _historyTitleText = string.Empty;
    private string _expressionLabelText = string.Empty;
    private string _resultLabelText = string.Empty;

    public MainViewModel()
    {
        _engine = new CalculatorEngine();

        HistoryItems = new ObservableCollection<CalculationHistoryItem>();

        AppendCommand = new RelayCommand(p => AppendToken(p as string ?? string.Empty));
        ClearAllCommand = new RelayCommand(_ => ClearAll());
        ClearEntryCommand = new RelayCommand(_ => ClearEntry());
        BackspaceCommand = new RelayCommand(_ => Backspace());
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        ToggleLanguageCommand = new RelayCommand(_ => ToggleLanguage());
        UseHistoryCommand = new RelayCommand(p => UseHistory(p as CalculationHistoryItem));

        _evaluateCommand = new RelayCommand(_ => EvaluateAndCommit(), _ => !string.IsNullOrWhiteSpace(Expression));
        EvaluateCommand = _evaluateCommand;

        _currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
            ? UiLanguage.Ru : UiLanguage.En;

        ApplyLocalization();
        UpdateThemeColors();
        RefreshHistory();
    }

    public ObservableCollection<CalculationHistoryItem> HistoryItems { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Expression
    {
        get => _expression;
        set
        {
            if (SetProperty(ref _expression, value ?? string.Empty))
                _evaluateCommand.RaiseCanExecuteChanged();
        }
    }

    public string ResultText
    {
        get => _resultText;
        set => SetProperty(ref _resultText, value ?? string.Empty);
    }

    public string PageBackground { get => _pageBackground; private set => SetProperty(ref _pageBackground, value); }
    public string PanelBackground { get => _panelBackground; private set => SetProperty(ref _panelBackground, value); }
    public string KeyBackground { get => _keyBackground; private set => SetProperty(ref _keyBackground, value); }
    public string HoverKeyBackground { get => _hoverKeyBackground; private set => SetProperty(ref _hoverKeyBackground, value); }
    public string PressedKeyBackground { get => _pressedKeyBackground; private set => SetProperty(ref _pressedKeyBackground, value); }
    public string KeyBorderBrush { get => _keyBorderBrush; private set => SetProperty(ref _keyBorderBrush, value); }
    public string KeyForeground { get => _keyForeground; private set => SetProperty(ref _keyForeground, value); }
    public string AccentKeyBackground { get => _accentKeyBackground; private set => SetProperty(ref _accentKeyBackground, value); }
    public string HoverAccentKeyBackground { get => _hoverAccentKeyBackground; private set => SetProperty(ref _hoverAccentKeyBackground, value); }
    public string PressedAccentKeyBackground { get => _pressedAccentKeyBackground; private set => SetProperty(ref _pressedAccentKeyBackground, value); }
    public string AccentKeyForeground { get => _accentKeyForeground; private set => SetProperty(ref _accentKeyForeground, value); }
    public string SecondaryText { get => _secondaryText; private set => SetProperty(ref _secondaryText, value); }

    public string WindowTitle { get => _windowTitle; private set => SetProperty(ref _windowTitle, value); }
    public string ThemeButtonText { get => _themeButtonText; private set => SetProperty(ref _themeButtonText, value); }
    public string LanguageButtonText { get => _languageButtonText; private set => SetProperty(ref _languageButtonText, value); }
    public string HistoryTitleText { get => _historyTitleText; private set => SetProperty(ref _historyTitleText, value); }
    public string ExpressionLabelText { get => _expressionLabelText; private set => SetProperty(ref _expressionLabelText, value); }
    public string ResultLabelText { get => _resultLabelText; private set => SetProperty(ref _resultLabelText, value); }

    public ICommand AppendCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand ClearEntryCommand { get; }
    public ICommand EvaluateCommand { get; }
    public ICommand BackspaceCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ToggleLanguageCommand { get; }
    public ICommand UseHistoryCommand { get; }

    public void Persist() => _engine.SaveAll();

    public void AppendToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
            Expression += token;
    }

    public void Backspace()
    {
        if (!string.IsNullOrEmpty(Expression))
            Expression = Expression[..^1];
    }

    private void EvaluateAndCommit()
    {
        if (string.IsNullOrWhiteSpace(Expression))
            return;

        try
        {
            var result = _engine.Calculate(Expression);
            ResultText = result.ToString(CultureInfo.InvariantCulture);
            RefreshHistory();
        }
        catch (Exception ex)
        {
            ResultText = $"Error: {ex.Message}";
        }
    }

    private void ClearAll()
    {
        Expression = string.Empty;
        ResultText = string.Empty;
    }

    private void ClearEntry()
    {
        if (string.IsNullOrWhiteSpace(Expression))
            return;

        var expr = Expression;
        int end = expr.Length - 1;

        while (end >= 0 && (char.IsDigit(expr[end]) || expr[end] == '.' || expr[end] == ','))
            end--;

        var removeStart = end + 1;

        if (removeStart > 0 && expr[removeStart - 1] == '-')
        {
            if (removeStart - 1 == 0)
                removeStart--;
            else
            {
                char prev = expr[removeStart - 2];
                if (!char.IsDigit(prev) && prev != '.' && prev != ',' && prev != ')')
                    removeStart--;
            }
        }

        Expression = expr.Remove(removeStart);
        ResultText = string.Empty;
    }

    private void ToggleTheme()
    {
        var current = _engine.GetThemeSettings();
        _engine.UpdateThemeSettings(new ThemeSettings(!current.IsDarkTheme, current.AccentColor));
        UpdateThemeColors();
    }

    private void ToggleLanguage()
    {
        _currentLanguage = _currentLanguage == UiLanguage.Ru ? UiLanguage.En : UiLanguage.Ru;
        ApplyLocalization();
    }

    private void UseHistory(CalculationHistoryItem? item)
    {
        if (item is null) return;

        Expression = item.Expression;
        try
        {
            var result = _engine.Evaluate(Expression);
            ResultText = result.ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            ResultText = $"Error: {ex.Message}";
        }
    }

    private void RefreshHistory()
    {
        HistoryItems.Clear();
        var history = _engine.GetHistory();
        for (int i = history.Count - 1; i >= 0; i--)
            HistoryItems.Add(history[i]);
    }

    private void UpdateThemeColors()
    {
        var settings = _engine.GetThemeSettings();
        var accent = settings.AccentColor;

        if (settings.IsDarkTheme)
        {
            PageBackground = "#0F1117";
            PanelBackground = "#1B1D26";
            KeyBackground = "#212633";
            HoverKeyBackground = "#2C3141";
            PressedKeyBackground = "#181C28";
            KeyBorderBrush = "#373D4E";
            KeyForeground = "#E6EAF2";
            SecondaryText = "#A8B0C0";
        }
        else
        {
            PageBackground = "#DADEE6";
            PanelBackground = "#E4E8F0";
            KeyBackground = "#D2D7E1";
            HoverKeyBackground = "#C8CDD8";
            PressedKeyBackground = "#BCBECE";
            KeyBorderBrush = "#BEC4D2";
            KeyForeground = "#161C2A";
            SecondaryText = "#505A70";
        }

        AccentKeyBackground = accent;
        HoverAccentKeyBackground = AdjustHex(accent, settings.IsDarkTheme ? 1.12 : 1.08);
        PressedAccentKeyBackground = AdjustHex(accent, settings.IsDarkTheme ? 0.86 : 0.90);
        AccentKeyForeground = "#FFFFFF";
    }

    private void ApplyLocalization()
    {
        if (_currentLanguage == UiLanguage.Ru)
        {
            WindowTitle = "Калькулятор Бахматова";
            ThemeButtonText = "Тема";
            LanguageButtonText = "EN";
            HistoryTitleText = "История";
            ExpressionLabelText = "Выражение";
            ResultLabelText = "Результат";
            return;
        }

        WindowTitle = "Calculator by Bakhmatov";
        ThemeButtonText = "Theme";
        LanguageButtonText = "RU";
        HistoryTitleText = "History";
        ExpressionLabelText = "Expression";
        ResultLabelText = "Result";
    }

    private static string AdjustHex(string hex, double factor)
    {
        var (r, g, b) = ParseHex(hex);
        byte Scale(byte ch) => (byte)Math.Clamp((int)(ch * factor), 0, 255);
        return $"#{Scale(r):X2}{Scale(g):X2}{Scale(b):X2}";
    }

    private static (byte r, byte g, byte b) ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        return (
            Convert.ToByte(hex[0..2], 16),
            Convert.ToByte(hex[2..4], 16),
            Convert.ToByte(hex[4..6], 16)
        );
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

