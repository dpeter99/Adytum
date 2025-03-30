namespace ConfigurationManager;

/// <summary>
/// UI formatting helper methods (replacing utils.sh/gum functionality)
/// </summary>
static class UI
{
    public static void ShowBanner(string message)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(new string('=', Console.WindowWidth - 1));
        Console.WriteLine(CenterText(message));
        Console.WriteLine(new string('=', Console.WindowWidth - 1));
        Console.ResetColor();
        Console.WriteLine();
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
        Console.ForegroundColor = ConsoleColor.DarkGray;
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