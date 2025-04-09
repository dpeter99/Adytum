using Microsoft.Extensions.Options;
using Spectre.Console;

namespace ConfigurationManager;

/// <summary>
/// UI formatting helper methods (replacing utils.sh/gum functionality)
/// </summary>
public class UI(IOptions<GlobalOptions> opts)
{
    
    
    public void ShowBanner(string message)
    {
        var panel = new Panel(message);
        panel.Width = 100;
        AnsiConsole.Write(panel);
    }
        
    public static void ShowSuccess(string message)
    {
        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(CenterText(message));
        Console.ResetColor();
        Console.WriteLine();
    }
        
    public static void Section(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine();
    }
        
    public static void Task(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
    }
        
    public static void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"ℹ {message}");
        Console.ResetColor();
    }
        
    public static void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"! WARNING: {message}");
        Console.ResetColor();
    }
        
    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ ERROR: {message}");
        Console.ResetColor();
    }
        
    public static void Debug(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"DEBUG: {message}");
        Console.ResetColor();
    }
        
    public static void ModuleHeader(string moduleName)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(new string('-', Console.WindowWidth - 1));
        Console.WriteLine(CenterText($"Module: {moduleName}"));
        Console.WriteLine(new string('-', Console.WindowWidth - 1));
        Console.ResetColor();
    }
        
    public static void Separator()
    {
        Console.WriteLine(new string('-', Console.WindowWidth - 1));
    }
        
    private static string CenterText(string text)
    {
        int padding = (Console.WindowWidth - text.Length) / 2;
        return text.PadLeft(padding + text.Length).PadRight(Console.WindowWidth - 1);
    }
}