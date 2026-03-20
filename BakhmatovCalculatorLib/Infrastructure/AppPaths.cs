namespace BakhmatovCalculatorLib.Infrastructure;

internal static class AppPaths
{
    private const string AppFolderName = "BakhmatovCalculator";

    public static string GetFilePath(string fileName)
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(baseDir, AppFolderName, fileName);
    }
}