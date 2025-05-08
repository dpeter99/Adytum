using System.Diagnostics;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Parsing;
using Serilog.Sinks.SystemConsole;
using Serilog.Sinks.SystemConsole.Output;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Templates;
using Spectre.Console;

namespace ConfigurationManager.Serilog.Spectre;

public class SpectreSink : ILogEventSink
{
    ITextFormatter _textFormatter;

    public SpectreSink(string template = "[{@t:HH:mm:ss} {@l:u3}] {@m}")
    {
        _textFormatter = new ExpressionTemplate(template);
    }
    
    
    public void Emit(LogEvent logEvent)
    {
        StringWriter stringWriter = new StringWriter();
        
        _textFormatter.Format(logEvent, stringWriter);
        
        var message = stringWriter.ToString();
        
        var line = logEvent.Level switch
        {
            LogEventLevel.Verbose => $"[grey]{Markup.Escape(message)}[/]",
            LogEventLevel.Debug => $"[grey42]{Markup.Escape(message)}[/]",
            LogEventLevel.Information => $"[white]{Markup.Escape(message)}[/]",
            LogEventLevel.Warning => $"[yellow]{Markup.Escape(message)}[/]",
            LogEventLevel.Error => $"[red]{Markup.Escape(message)}[/]",
            LogEventLevel.Fatal => $"[red1]{Markup.Escape(message)}[/]",
            _ => $"{Markup.Escape(logEvent.Level.ToString())} {Markup.Escape(message)}"
        };
        
        //Debugger.Break();

        AnsiConsole.MarkupLine(line);
        
    }
    
}

  

// public static class LoggerConfigurationExtensions{
//
//     public static LoggerConfiguration Console(
//         this LoggerSinkConfiguration sinkConfiguration,
//         LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
//         string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
//         IFormatProvider? formatProvider = null,
//         LoggingLevelSwitch? levelSwitch = null,
//         LogEventLevel? standardErrorFromLevel = null,
//         ConsoleTheme? theme = null,
//         bool applyThemeToRedirectedOutput = false,
//         object? syncRoot = null)
//     {
//         if (sinkConfiguration == null)
//             throw new ArgumentNullException(nameof (sinkConfiguration));
//         if (outputTemplate == null)
//             throw new ArgumentNullException(nameof (outputTemplate));
//         ConsoleTheme theme1 = !applyThemeToRedirectedOutput && (System.Console.IsOutputRedirected || System.Console.IsErrorRedirected) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR")) ? ConsoleTheme.None : theme ?? (ConsoleTheme) SystemConsoleThemes.Literate;
//         if (syncRoot == null)
//             syncRoot = ConsoleLoggerConfigurationExtensions.DefaultSyncRoot;
//         OutputTemplateRenderer formatter = new OutputTemplateRenderer(theme1, outputTemplate, formatProvider);
//         return sinkConfiguration.Sink((ILogEventSink) new ConsoleSink(theme1, (ITextFormatter) formatter, standardErrorFromLevel, syncRoot), restrictedToMinimumLevel, levelSwitch);
//     }
//     
// }