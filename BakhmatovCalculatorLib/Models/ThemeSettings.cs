namespace BakhmatovCalculatorLib.Models;

public sealed class ThemeSettings
{
    public bool IsDarkTheme { get; set; } = true;
    public string AccentColor { get; set; } = "#1E90FF";

    public ThemeSettings()
    {
    }

    public ThemeSettings(bool isDarkTheme, string accentColor)
    {
        IsDarkTheme = isDarkTheme;
        AccentColor = accentColor;
    }

    public static ThemeSettings Default => new ThemeSettings(isDarkTheme: true, accentColor: "#1E90FF");
}

