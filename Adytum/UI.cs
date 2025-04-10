using Microsoft.Extensions.Options;
using Spectre.Console;

namespace ConfigurationManager;

/// <summary>
/// UI formatting helper methods using Spectre.Console for rich terminal output
/// </summary>
public class UI
{
    private readonly GlobalOptions _options;

    public UI(IOptions<GlobalOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Display a styled banner with the given message
    /// </summary>
    public void ShowBanner(string message)
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

    /// <summary>
    /// Display information text
    /// </summary>
    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"[grey]ℹ {Markup.Escape(message)}[/]");
    }

    /// <summary>
    /// Display a warning message
    /// </summary>
    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]! WARNING:[/] [yellow]{Markup.Escape(message)}[/]");
    }

    /// <summary>
    /// Display an error message
    /// </summary>
    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]✗ ERROR:[/] [red]{Markup.Escape(message)}[/]");
    }

    /// <summary>
    /// Display debug information (only if debug is enabled)
    /// </summary>
    public void Debug(string message)
    {
        if (_options.Debug)
        {
            AnsiConsole.MarkupLine($"[grey42]DEBUG: {Markup.Escape(message)}[/]");
        }
    }

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
    public static async Task<T> SpinnerAsync<T>(string message, Func<Task<T>> action)
    {
        return await AnsiConsole.Status()
            .Spinner(Spectre.Console.Spinner.Known.Dots)
            .StartAsync(message, async ctx => await action());
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