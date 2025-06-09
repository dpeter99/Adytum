using System.Reflection;
using ConfigurationManager.ConsoleApp;

namespace ConfigurationManager.Commands;

public class VersionCommand
{
    [Command("version", Description = "Displays the application version information.")]
    public int Run()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        UI.ShowBanner($"Adytum version: {version}");
        return 0;
    }
}


