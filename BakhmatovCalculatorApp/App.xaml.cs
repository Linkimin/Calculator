using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

namespace BakhmatovCalculatorApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            File.WriteAllText("crash.log", ex.ExceptionObject.ToString());
        };
        base.OnStartup(e);
    }
}

