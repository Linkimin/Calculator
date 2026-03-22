using BakhmatovCalculatorLib.Models;
using BakhmatovCalculatorLib.Services;

namespace BakhmatovCalculatorTests;

public sealed class HistoryServiceTests
{
    private static HistoryService CreateService()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "BakhmatovCalculatorTests",
            Guid.NewGuid().ToString("N"),
            "history.json");
        return new HistoryService(path);
    }

    [Fact]
    public void Load_WhenFileDoesNotExist_ReturnsEmpty()
    {
        var service = CreateService();
        var result = service.Load();
        Assert.Empty(result);
    }

    [Fact]
    public void Save_And_Load_PersistsItems()
    {
        var service = CreateService();
        var items = new List<CalculationHistoryItem>
        {
            new("2+2", 4, DateTime.UtcNow),
            new("3*3", 9, DateTime.UtcNow),
        };

        service.Save(items);
        var loaded = service.Load();

        Assert.Equal(2, loaded.Count);
        Assert.Equal("2+2", loaded[0].Expression);
        Assert.Equal(4m, loaded[0].Result);
        Assert.Equal("3*3", loaded[1].Expression);
        Assert.Equal(9m, loaded[1].Result);
    }

    [Fact]
    public void Save_EmptyList_LoadReturnsEmpty()
    {
        var service = CreateService();
        service.Save(new List<CalculationHistoryItem>());
        var loaded = service.Load();
        Assert.Empty(loaded);
    }

    [Fact]
    public void Save_OverwritesPreviousData()
    {
        var service = CreateService();

        service.Save(new List<CalculationHistoryItem>
        {
            new("1+1", 2, DateTime.UtcNow)
        });

        service.Save(new List<CalculationHistoryItem>
        {
            new("5+5", 10, DateTime.UtcNow)
        });

        var loaded = service.Load();
        Assert.Single(loaded);
        Assert.Equal("5+5", loaded[0].Expression);
    }

    [Fact]
    public void Load_CorruptedFile_ReturnsEmpty()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "BakhmatovCalculatorTests",
            Guid.NewGuid().ToString("N"),
            "history.json");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "this is not valid json {{{{");

        var service = new HistoryService(path);
        var result = service.Load();
        Assert.Empty(result);
    }

    [Fact]
    public void Timestamps_ArePersisted()
    {
        var service = CreateService();
        var timestamp = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        service.Save(new List<CalculationHistoryItem>
        {
            new("1+1", 2, timestamp)
        });

        var loaded = service.Load();
        Assert.Equal(timestamp, loaded[0].Timestamp);
    }
}
