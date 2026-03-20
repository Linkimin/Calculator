using System.Text.Json;
using BakhmatovCalculatorLib.Models;

namespace BakhmatovCalculatorLib.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _filePath;

    public SettingsService(string? filePath = null)
    {
        _filePath = filePath ?? GetDefaultFilePath("settings.json");
    }

    public ThemeSettings Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return ThemeSettings.Default;

            using var stream = File.OpenRead(_filePath);
            var settings = JsonSerializer.Deserialize<ThemeSettings>(stream, JsonOptions);
            return settings ?? ThemeSettings.Default;
        }
        catch
        {
            return ThemeSettings.Default;
        }
    }

    public void Save(ThemeSettings settings)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions(JsonOptions) { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    private static string GetDefaultFilePath(string fileName)
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDir = Path.Combine(baseDir, "BakhmatovCalculator");
        return Path.Combine(appDir, fileName);
    }
}

