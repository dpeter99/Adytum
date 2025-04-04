using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.Reflection;
using Cocona;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConfigurationManager.ConsoleApp;

public interface ICommandProvider
{
    static abstract Command GetCommand(IConfiguration configuration);
}

public class ConsoleAppHostBuilder : IHostApplicationBuilder
{
    private HostApplicationBuilder _hostBuilder;
    
    private CommandLineBuilder _commandLineBuilder = new();
    
    public ConsoleAppHostBuilder(HostApplicationBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder;
    }
    
    public void AddHelpCommand()
    {
        _commandLineBuilder.UseHelp();
    }

    public void AddCommand(Command command)
    {
        _commandLineBuilder.Command.Add(command);
    }
    
    public void AddCommand<TCommand>()
    {
        var type = typeof(TCommand);
        Console.WriteLine("Type:" + type.Name);
        
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToList().Select(info =>
            {
                return (info, attr: info.GetCustomAttribute<CommandAttribute>());
            })
            .Where((i, _) => i.attr is not null);
        
        foreach (var (method, attr) in methods)
        {
            var c = ClassCommandMapper.CreateCommand(attr, method, type);

            _commandLineBuilder.Command.Add(c);
        }
    }


    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
    {
        _hostBuilder.ConfigureContainer(factory, configure);
    }

    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_hostBuilder).Properties;

    public IConfigurationManager Configuration => _hostBuilder.Configuration;

    public IHostEnvironment Environment => _hostBuilder.Environment;

    public ILoggingBuilder Logging => _hostBuilder.Logging;

    public IMetricsBuilder Metrics => _hostBuilder.Metrics;

    public IServiceCollection Services => _hostBuilder.Services;
    
    public ConsoleAppHost Build()
    {
        var host = _hostBuilder.Build();

        _commandLineBuilder.AddMiddleware((context =>
        {
            context.BindingContext.AddService(typeof(IHost), _ => host);
        }));
        
        return new ConsoleAppHost(host, _commandLineBuilder.Build());
    }
    
}