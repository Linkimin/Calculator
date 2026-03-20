using System.Text.Json;
using BakhmatovCalculatorLib.Models;

namespace BakhmatovCalculatorLib.Services;

public sealed class HistoryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _filePath;

    public HistoryService(string? filePath = null)
    {
        _filePath = filePath ?? GetDefaultFilePath("history.json");
    }

    public IReadOnlyList<CalculationHistoryItem> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return Array.Empty<CalculationHistoryItem>();

            using var stream = File.OpenRead(_filePath);
            var items = JsonSerializer.Deserialize<List<CalculationHistoryItem>>(stream, JsonOptions);
            return items ?? new List<CalculationHistoryItem>();
        }
        catch
        {
            // If something goes wrong (corrupted file, permission issues, etc.), do not crash the app.
            return Array.Empty<CalculationHistoryItem>();
        }
    }

    public void Save(IEnumerable<CalculationHistoryItem> items)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions(JsonOptions) { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    private static string GetDefaultFilePath(string fileName)
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDir = Path.Combine(baseDir, "BakhmatovCalculator");
        return Path.Combine(appDir, fileName);
    }
}

