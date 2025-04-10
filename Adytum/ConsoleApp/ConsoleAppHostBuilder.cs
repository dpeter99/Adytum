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
using Microsoft.Extensions.Primitives;

namespace ConfigurationManager.ConsoleApp;

public interface ICommandProvider
{
    static abstract Command GetCommand(IConfiguration configuration);
}

public class ConsoleAppHostBuilder : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _hostBuilder;
    
    private readonly CommandLineBuilder _commandLineBuilder = new();
    
    public ConsoleAppHostBuilder(HostApplicationBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder;
    }

    public void AddGlobalOptions<T>(string[] args) where T : class, new()
    {
        IConfigurationBuilder builder = _hostBuilder.Configuration;
        
        builder.Add(new CommandLineConfigSource<T>(typeof(T).Name,args, _commandLineBuilder.Command));

        //_hostBuilder.Services.AddOptions<T>(typeof(T).Name);
        _hostBuilder.Services.Configure<T>(_hostBuilder.Configuration.GetSection(typeof(T).Name));
    }
    
    public void AddCommand<TCommand>() where TCommand : class
    {
        var type = typeof(TCommand);

        _hostBuilder.Services.AddTransient<TCommand>();
        
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
    
    #region IHostApplicationBuilder
    
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
    
    #endregion IHostApplicationBuilder
    
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

public class Reflector
{
    public static Dictionary<Option, PropertyInfo> MapOptions(Type type)
    {
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        Dictionary<Option, PropertyInfo> map = new();
        foreach (var prop in props)
        {
            var attribute = prop.GetCustomAttribute<OptionAttribute>();
            if (attribute is null)
                continue;
            
            var option = CreateOption(prop, attribute);
            if(option != null)
                map.Add(
                    option,
                    prop
                );
        }
        return map;
    }
    
    private static Option? CreateOption(PropertyInfo parm, OptionAttribute attribute)
    {
        var name = "--" + attribute.Name ?? parm.Name;
        var description = attribute.Description ?? parm.Name;
        
        var constructor = typeof(Option<>)
            .MakeGenericType(parm.PropertyType)
            .GetConstructor(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance,
                new[] { typeof(string), typeof(string) });

        if (constructor?.Invoke([name, description]) is not Option option)
            return null;
        
        foreach (var shortName in attribute.ShortNames)
        {
            option.AddAlias("-"+shortName);            
        }
        
        Console.WriteLine("Option:" + option);
        return option;
    }
}

public class CommandLineConfigSource<T>(string path, string[] args, Command command) : IConfigurationSource where T : new()
{
    private T Parse()
    {
        var propsMap = Reflector.MapOptions(typeof(T));
        
        foreach (var (option, _) in propsMap)
        {
            command.AddOption(option);
        }
        
        var result = command.Parse(args);

        T data = new();
        foreach (var (option, prop) in propsMap)
        {
            var value = result.CommandResult.GetValueForOption(command.Options[0]);
            prop.SetValue(data, value);
        }

        return data;
    }
    
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        Console.WriteLine("CommandLineConfigSource.Build()");
        var value = this.Parse();
        return new CommandLineConfigProvider<T>(builder, path, value);
    }
}

public class CommandLineConfigProvider<T>(IConfigurationBuilder builder, string path, T value) : ConfigurationProvider
{
    public override void Load()
    {
        Data = new Dictionary<string, string?>();

        var props = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        foreach (var prop in props)
        {
            Data.Add(
                path + ":" + prop.Name,
                prop.GetValue(value)?.ToString()
                );
        }
    }
}


/// <summary>
/// Specifies the method that should be treated as a command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>Gets or sets the command name.</summary>
    public string? Name { get; }

    /// <summary>Gets or sets the command aliases.</summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets the command description.</summary>
    public string Description { get; set; } = string.Empty;

    public CommandAttribute()
    {
    }

    public CommandAttribute(string name) => this.Name = name;
}


[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class OptionAttribute : Attribute
{
    /// <summary>Gets or sets the option description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the option name. The name is long-form name. (e.g. "output", "force")
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets or sets the option short-form names. (e.g. 'O', 'f')
    /// </summary>
    public IReadOnlyList<char> ShortNames { get; } = (IReadOnlyList<char>) Array.Empty<char>();

    /// <summary>Gets the option value name.</summary>
    public string? ValueName { get; set; }

    /// <summary>
    /// Gets or sets whether to stop parsing options after this option on the command line.
    /// </summary>
    public bool StopParsingOptions { get; set; }

    public OptionAttribute()
    {
    }

    public OptionAttribute(string name, char[]? shortNames = null)
    {
        this.Name = name ?? throw new ArgumentNullException(nameof (name));
        this.ShortNames = (IReadOnlyList<char>) (shortNames ?? Array.Empty<char>());
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A name of option must have name.");
    }

    public OptionAttribute(char shortName)
    {
        this.ShortNames = (IReadOnlyList<char>) new char[1]
        {
            shortName
        };
    }

    public OptionAttribute(char[]? shortNames)
    {
        this.ShortNames = (IReadOnlyList<char>) (shortNames ?? Array.Empty<char>());
    }
}