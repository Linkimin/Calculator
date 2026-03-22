using BakhmatovCalculatorLib.Models;
using BakhmatovCalculatorLib.Services;

namespace BakhmatovCalculatorTests;

public sealed class SettingsServiceTests
{
    private static SettingsService CreateService()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "BakhmatovCalculatorTests",
            Guid.NewGuid().ToString("N"),
            "settings.json");
        return new SettingsService(path);
    }

    [Fact]
    public void Load_WhenFileDoesNotExist_ReturnsDefault()
    {
        var service = CreateService();
        var result = service.Load();

        Assert.True(result.IsDarkTheme);
        Assert.Equal("#1E90FF", result.AccentColor);
    }

    [Fact]
    public void Save_And_Load_PersistsSettings()
    {
        var service = CreateService();
        var settings = new ThemeSettings(isDarkTheme: false, accentColor: "#FF0000");

        service.Save(settings);
        var loaded = service.Load();

        Assert.False(loaded.IsDarkTheme);
        Assert.Equal("#FF0000", loaded.AccentColor);
    }

    [Fact]
    public void Save_OverwritesPreviousSettings()
    {
        var service = CreateService();

        service.Save(new ThemeSettings(isDarkTheme: true, accentColor: "#AABBCC"));
        service.Save(new ThemeSettings(isDarkTheme: false, accentColor: "#112233"));

        var loaded = service.Load();
        Assert.False(loaded.IsDarkTheme);
        Assert.Equal("#112233", loaded.AccentColor);
    }

    [Fact]
    public void Load_CorruptedFile_ReturnsDefault()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "BakhmatovCalculatorTests",
            Guid.NewGuid().ToString("N"),
            "settings.json");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ invalid json }");

        var service = new SettingsService(path);
        var result = service.Load();

        Assert.True(result.IsDarkTheme);
        Assert.Equal("#1E90FF", result.AccentColor);
    }

    [Fact]
    public void Default_Property_ReturnsDarkThemeWithBlueAccent()
    {
        var def = ThemeSettings.Default;
        Assert.True(def.IsDarkTheme);
        Assert.Equal("#1E90FF", def.AccentColor);
    }
}
