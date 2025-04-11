using Microsoft.Extensions.Options;
using Spectre.Console;

namespace ConfigurationManager;

public enum PrintLevel
{
    /// <summary>
    /// Anything and everything you might want to know about
    /// a running block of code.
    /// </summary>
    Verbose,

    /// <summary>
    /// Internal system events that aren't necessarily
    /// observable from the outside.
    /// </summary>
    Debug,

    /// <summary>
    /// The lifeblood of operational intelligence - things
    /// happen.
    /// </summary>
    Information,

    /// <summary>
    /// Service is degraded or endangered.
    /// </summary>
    Warning,

    /// <summary>
    /// Functionality is unavailable, invariants are broken
    /// or data is lost.
    /// </summary>
    Error,

    /// <summary>
    /// If you have a pager, it goes off when one of these
    /// occurs.
    /// </summary>
    Fatal
}

/// <summary>
/// UI formatting helper methods using Spectre.Console for rich terminal output
/// </summary>
public static class UI
{

    /// <summary>
    /// Display a styled banner with the given message
    /// </summary>
    public static void ShowBanner(string message)
    {
        var panel = new Panel(Markup.Escape(message))
            .Border(BoxBorder.Double)
            .BorderStyle(new Style(Color.Magenta1))
            .Padding(1, 2)
            .Header("[bold magenta]Adytum Setup[/]")
            .HeaderAlignment(Justify.Center);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display a success banner
    /// </summary>
    public static void ShowSuccess(string message)
    {
        var panel = new Panel(Markup.Escape(message))
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Green))
            .Padding(0, 2)
            .Header("[bold green]SUCCESS[/]")
            .HeaderAlignment(Justify.Center);

        panel.Expand = true;
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display a section header
    /// </summary>
    public static void Section(string message)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold cyan]{Markup.Escape(message)}[/]").RuleStyle(Style.Parse("cyan")).DoubleBorder());
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display a completed task
    /// </summary>
    public static void Task(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓[/] [green]{Markup.Escape(message)}[/]");
    }

    /// <summary>
    /// Display a subtask
    /// </summary>
    public static void SubTask(string message)
    {
        AnsiConsole.MarkupLine($"[blue]•[/] [blue]{Markup.Escape(message)}[/]");
    }

    public static void PrintLog(PrintLevel level, string message)
    {
        var line = level switch
        {
            PrintLevel.Verbose => $"[grey] {Markup.Escape(message)}[/]",
            PrintLevel.Debug => $"[grey42]DEBUG: {Markup.Escape(message)}[/]",
            PrintLevel.Information => $"[white] {Markup.Escape(message)}[/]",
            PrintLevel.Warning => $"[yellow]! WARNING:[/] [yellow]{Markup.Escape(message)}[/]",
            PrintLevel.Error => $"[red]✗ ERROR:[/] [red]{Markup.Escape(message)}[/]",
            PrintLevel.Fatal => $"[red1] {Markup.Escape(message)}[/]",
            _ => $"{Markup.Escape(level.ToString())} {Markup.Escape(message)}"
        };

        AnsiConsole.MarkupLine(line);
    }
    
    /// <summary>
    /// Display debug information (only if debug is enabled)
    /// </summary>
    public static void Debug(string message) => PrintLog(PrintLevel.Debug, message);
    
    /// <summary>
    /// Display information text
    /// </summary>
    public static void Info(string message) => PrintLog(PrintLevel.Information, message);

    /// <summary>
    /// Display a warning message
    /// </summary>
    public static void Warning(string message) => PrintLog(PrintLevel.Warning, message);

    /// <summary>
    /// Display an error message
    /// </summary>
    public static void Error(string message) => PrintLog(PrintLevel.Error, message);



    /// <summary>
    /// Display a module header
    /// </summary>
    public static void ModuleHeader(string moduleName)
    {
        var panel = new Panel($"[bold white]Module: {Markup.Escape(moduleName)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Style.Parse("grey"))
            .Expand()
            .PadBottom(1);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Display a horizontal separator
    /// </summary>
    public static void Separator()
    {
        AnsiConsole.Write(new Rule().RuleStyle(Style.Parse("grey")));
    }

    /// <summary>
    /// Run a task with a spinner
    /// </summary>
    public static T Spinner<T>(string message, Func<T> action)
    {
        return AnsiConsole.Status()
            .Spinner(Spectre.Console.Spinner.Known.Dots)
            .Start(message, ctx => action());
    }

    /// <summary>
    /// Run a task with a spinner (async version)
    /// </summary>
    public static async Task<T> SpinnerAsync<T>(string message, Func<StatusContext, Task<T>> action)
    {
        return await AnsiConsole.Status()
            .Spinner(Spectre.Console.Spinner.Known.Dots)
            .StartAsync(message, async ctx => await action(ctx));
    }

    /// <summary>
    /// Allow user to select multiple items from a list
    /// </summary>
    public static List<string> MultiSelect(string title, IEnumerable<string> choices)
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title(title)
                .PageSize(15)
                .HighlightStyle(new Style(Color.Green))
                .InstructionsText("[grey](Press [green]<space>[/] to select, [green]<enter>[/] to accept)[/]")
                .AddChoices(choices));
    }

    /// <summary>
    /// Ask user for confirmation
    /// </summary>
    public static bool Confirm(string question)
    {
        return AnsiConsole.Confirm(question);
    }

    /// <summary>
    /// Display a progress bar for a set of tasks
    /// </summary>
    public static void Progress(string title, List<(string Description, Action Task)> tasks)
    {
        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn())
            .Start(ctx =>
            {
                foreach (var (description, task) in tasks)
                {
                    var progressTask = ctx.AddTask($"[green]{Markup.Escape(description)}[/]");
                    task();
                    progressTask.Value = 100;
                }
            });
    }
}