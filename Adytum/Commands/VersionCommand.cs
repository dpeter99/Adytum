using System.Reflection;
using ConfigurationManager.ConsoleApp;

namespace ConfigurationManager.Commands;

public class VersionCommand
{
    [Command("version", Description = "Displays the application version information.")]
    public int Run()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? 
                     assembly.GetName().Version?.ToString() ?? 
                     "unknown";
        UI.ShowBanner($"Adytum version: {version}");
        return 0;
    }
}


