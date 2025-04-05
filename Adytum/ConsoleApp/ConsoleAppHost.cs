using System.CommandLine.Parsing;
using Microsoft.Extensions.Hosting;

namespace ConfigurationManager.ConsoleApp;

public class ConsoleAppHost: IHost
{
    private IHost _host;
    private readonly Parser _parser;

    public ConsoleAppHost(IHost host, Parser parser)
    {
        _host = host;
        _parser = parser;
    }

    public void Dispose()
    {
        _host.Dispose();
    }

    public async Task Start(string[] args)
    {
        var a = _parser.Parse(args);
        Console.WriteLine("Parsed");
        if (a.Errors.Any())
        {
            Console.WriteLine(string.Join(Environment.NewLine, a.Errors));
        }
        else 
            await a.InvokeAsync();
    }
    
    public Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return _host.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return _host.StopAsync(cancellationToken);
    }

    public IServiceProvider Services => _host.Services;
}