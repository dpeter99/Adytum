using Serilog.Core;
using Serilog.Events;

namespace ConfigurationManager.Utils;

public class UILogSink : ILogEventSink
{
    
    
    public void Emit(LogEvent logEvent)
    {
        switch(logEvent.Level)
        {
            case LogEventLevel.Verbose:
                UI.PrintLog(PrintLevel.Verbose, logEvent.RenderMessage());
                break;
            case LogEventLevel.Debug:
                UI.PrintLog(PrintLevel.Debug, logEvent.RenderMessage());
                break;
            case LogEventLevel.Information:
                UI.PrintLog(PrintLevel.Information, logEvent.RenderMessage());
                break;
            case LogEventLevel.Warning:
                UI.PrintLog(PrintLevel.Warning, logEvent.RenderMessage());
                break;
            case LogEventLevel.Error:
                UI.PrintLog(PrintLevel.Error, logEvent.RenderMessage());
                break;
            case LogEventLevel.Fatal:
                UI.PrintLog(PrintLevel.Fatal, logEvent.RenderMessage());
                break;
            default:
                Console.WriteLine(logEvent.RenderMessage());
                break;
        }
    }
}