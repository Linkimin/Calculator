using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using BakhmatovCalculatorLib.Calculators;
using BakhmatovCalculatorLib.Models;
using BakhmatovCalculatorLib.Services;
using BakhmatovCalculatorApp.Infrastructure;

namespace BakhmatovCalculatorApp.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private enum UiLanguage
    {
        Ru,
        En
    }

    private readonly CalculatorEngine _engine;
    private UiLanguage _currentLanguage;

    private string _expression = string.Empty;
    private string _resultText = string.Empty;

    private Brush _pageBackground = Brushes.Black;
    private Brush _panelBackground = Brushes.Transparent;

    private Brush _keyBackground = Brushes.Transparent;
    private Brush _hoverKeyBackground = Brushes.Transparent;
    private Brush _pressedKeyBackground = Brushes.Transparent;
    private Brush _keyBorderBrush = Brushes.Transparent;
    private Brush _keyForeground = Brushes.Black;

    private Brush _accentKeyBackground = Brushes.DeepSkyBlue;
    private Brush _hoverAccentKeyBackground = Brushes.DeepSkyBlue;
    private Brush _pressedAccentKeyBackground = Brushes.DeepSkyBlue;
    private Brush _accentKeyForeground = Brushes.White;

    private Brush _secondaryText = Brushes.Gray;
    private string _windowTitle = string.Empty;
    private string _themeButtonText = string.Empty;
    private string _languageButtonText = string.Empty;
    private string _historyTitleText = string.Empty;
    private string _expressionLabelText = string.Empty;
    private string _resultLabelText = string.Empty;

    public MainViewModel()
    {
        // Let the library decide default persistence locations.
        _engine = new CalculatorEngine();

        HistoryItems = new ObservableCollection<CalculationHistoryItem>();

        AppendCommand = new RelayCommand(p => AppendToken(p as string ?? string.Empty));
        ClearAllCommand = new RelayCommand(_ => ClearAll());
        ClearEntryCommand = new RelayCommand(_ => ClearEntry());
        BackspaceCommand = new RelayCommand(_ => Backspace());

        _evaluateCommand = new RelayCommand(_ => EvaluateAndCommit(), _ => !string.IsNullOrWhiteSpace(Expression));
        EvaluateCommand = _evaluateCommand;

        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        ToggleLanguageCommand = new RelayCommand(_ => ToggleLanguage());
        UseHistoryCommand = new RelayCommand(p => UseHistory(p as CalculationHistoryItem));

        _currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
            ? UiLanguage.Ru
            : UiLanguage.En;

        ApplyLocalization();
        UpdateThemeBrushes();
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

    public Brush PageBackground
    {
        get => _pageBackground;
        private set => SetProperty(ref _pageBackground, value);
    }

    public Brush PanelBackground
    {
        get => _panelBackground;
        private set => SetProperty(ref _panelBackground, value);
    }

    public Brush KeyBackground
    {
        get => _keyBackground;
        private set => SetProperty(ref _keyBackground, value);
    }

    public Brush HoverKeyBackground
    {
        get => _hoverKeyBackground;
        private set => SetProperty(ref _hoverKeyBackground, value);
    }

    public Brush PressedKeyBackground
    {
        get => _pressedKeyBackground;
        private set => SetProperty(ref _pressedKeyBackground, value);
    }

    public Brush KeyBorderBrush
    {
        get => _keyBorderBrush;
        private set => SetProperty(ref _keyBorderBrush, value);
    }

    public Brush KeyForeground
    {
        get => _keyForeground;
        private set => SetProperty(ref _keyForeground, value);
    }

    public Brush AccentKeyBackground
    {
        get => _accentKeyBackground;
        private set => SetProperty(ref _accentKeyBackground, value);
    }

    public Brush HoverAccentKeyBackground
    {
        get => _hoverAccentKeyBackground;
        private set => SetProperty(ref _hoverAccentKeyBackground, value);
    }

    public Brush PressedAccentKeyBackground
    {
        get => _pressedAccentKeyBackground;
        private set => SetProperty(ref _pressedAccentKeyBackground, value);
    }

    public Brush AccentKeyForeground
    {
        get => _accentKeyForeground;
        private set => SetProperty(ref _accentKeyForeground, value);
    }

    public Brush SecondaryText
    {
        get => _secondaryText;
        private set => SetProperty(ref _secondaryText, value);
    }

    public ICommand AppendCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand ClearEntryCommand { get; }
    public ICommand EvaluateCommand { get; }
    public ICommand BackspaceCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ToggleLanguageCommand { get; }
    public ICommand UseHistoryCommand { get; }
    public string WindowTitle
    {
        get => _windowTitle;
        private set => SetProperty(ref _windowTitle, value);
    }

    public string ThemeButtonText
    {
        get => _themeButtonText;
        private set => SetProperty(ref _themeButtonText, value);
    }

    public string LanguageButtonText
    {
        get => _languageButtonText;
        private set => SetProperty(ref _languageButtonText, value);
    }

    public string HistoryTitleText
    {
        get => _historyTitleText;
        private set => SetProperty(ref _historyTitleText, value);
    }

    public string ExpressionLabelText
    {
        get => _expressionLabelText;
        private set => SetProperty(ref _expressionLabelText, value);
    }

    public string ResultLabelText
    {
        get => _resultLabelText;
        private set => SetProperty(ref _resultLabelText, value);
    }


    private readonly RelayCommand _evaluateCommand;

    public void Persist() => _engine.SaveAll();

    public void AppendToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return;

        Expression += token;
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

    public void Backspace()
    {
        if (string.IsNullOrEmpty(Expression))
            return;

        Expression = Expression[..^1];
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

        // Remove last "number segment" at the end of the expression.
        // Example: "3*-2" -> remove "-2" (with its unary '-') but "5-2" -> remove "2".
        var expr = Expression;

        int end = expr.Length - 1;

        while (end >= 0 && (char.IsDigit(expr[end]) || expr[end] == '.' || expr[end] == ','))
            end--;

        var removeStart = end + 1;

        // If the number had a unary '-' sign, remove it too.
        if (removeStart > 0 && expr[removeStart - 1] == '-')
        {
            if (removeStart - 1 == 0)
            {
                removeStart--;
            }
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
        var next = new ThemeSettings(!current.IsDarkTheme, current.AccentColor);
        _engine.UpdateThemeSettings(next);
        UpdateThemeBrushes();
    }

    private void ToggleLanguage()
    {
        _currentLanguage = _currentLanguage == UiLanguage.Ru ? UiLanguage.En : UiLanguage.Ru;
        ApplyLocalization();
    }

    private void UseHistory(CalculationHistoryItem? item)
    {
        if (item is null)
            return;

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

        // Show newest first.
        var history = _engine.GetHistory();
        for (int i = history.Count - 1; i >= 0; i--)
            HistoryItems.Add(history[i]);
    }

    private void UpdateThemeBrushes()
    {
        var settings = _engine.GetThemeSettings();

        Color accentColor;
        try
        {
            var converted = ColorConverter.ConvertFromString(settings.AccentColor);
            accentColor = converted is Color c ? c : Color.FromRgb(30, 144, 255);
        }
        catch
        {
            accentColor = Color.FromRgb(30, 144, 255);
        }

        if (settings.IsDarkTheme)
        {
            PageBackground = new SolidColorBrush(Color.FromRgb(15, 17, 23));
            PanelBackground = new SolidColorBrush(Color.FromRgb(27, 29, 38));

            KeyBackground = new SolidColorBrush(Color.FromRgb(33, 38, 51));
            HoverKeyBackground = new SolidColorBrush(Color.FromRgb(44, 49, 65));
            PressedKeyBackground = new SolidColorBrush(Color.FromRgb(24, 28, 40));
            KeyBorderBrush = new SolidColorBrush(Color.FromRgb(55, 61, 78));
            KeyForeground = new SolidColorBrush(Color.FromRgb(230, 234, 242));

            AccentKeyBackground = new SolidColorBrush(accentColor);
            HoverAccentKeyBackground = new SolidColorBrush(AdjustColor(accentColor, 1.12));
            PressedAccentKeyBackground = new SolidColorBrush(AdjustColor(accentColor, 0.86));
            AccentKeyForeground = Brushes.White;

            SecondaryText = new SolidColorBrush(Color.FromRgb(168, 176, 192));
        }
        else
        {
            PageBackground = new SolidColorBrush(Color.FromRgb(245, 247, 251));
            PanelBackground = Brushes.White;

            KeyBackground = new SolidColorBrush(Color.FromRgb(239, 242, 247));
            HoverKeyBackground = new SolidColorBrush(Color.FromRgb(229, 234, 242));
            PressedKeyBackground = new SolidColorBrush(Color.FromRgb(217, 223, 233));
            KeyBorderBrush = new SolidColorBrush(Color.FromRgb(214, 218, 229));
            KeyForeground = new SolidColorBrush(Color.FromRgb(29, 36, 51));

            AccentKeyBackground = new SolidColorBrush(accentColor);
            HoverAccentKeyBackground = new SolidColorBrush(AdjustColor(accentColor, 1.08));
            PressedAccentKeyBackground = new SolidColorBrush(AdjustColor(accentColor, 0.9));
            AccentKeyForeground = Brushes.White;

            SecondaryText = new SolidColorBrush(Color.FromRgb(92, 100, 120));
        }
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

    private static Color AdjustColor(Color color, double factor)
    {
        byte Scale(byte channel)
        {
            var value = (int)(channel * factor);
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        return Color.FromRgb(Scale(color.R), Scale(color.G), Scale(color.B));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

