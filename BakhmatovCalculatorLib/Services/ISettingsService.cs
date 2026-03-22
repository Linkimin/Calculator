using BakhmatovCalculatorLib.Models;

namespace BakhmatovCalculatorLib.Services;

public interface ISettingsService
{
    ThemeSettings Load();
    void Save(ThemeSettings settings);
}
