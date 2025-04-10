using System.CommandLine;
using System.CommandLine.Hosting;
using System.Reflection;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfigurationManager.ConsoleApp;

public class ClassCommandMapper
{

    public static Command CreateCommand(CommandAttribute? attr, MethodInfo method, Type type)
    {
        var c = new Command(attr.Name ?? method.Name);
        c.Description = attr.Description ?? method.Name;
            
        method.GetParameters().Select(parm =>
        {
            var option = CreateOption(parm);

            return option;
        }).ToList().ForEach(option => c.AddOption(option));
            
        c.SetHandler(async context =>
        {
            var instance = context.GetHost().Services.GetRequiredService(type);

            var parameters = method.GetParameters();
            var args = parameters.Select(p => 
            {
                var optionAttribute = p.GetCustomAttribute<OptionAttribute>();
                if (optionAttribute is null) return null;
                
                var matchedOption = context.ParseResult.CommandResult.Command.Options
                    .FirstOrDefault(o => o.Name.TrimStart('-').ToLower() == optionAttribute.Name.ToLower());
                
                if (matchedOption != null)
                {
                    return context.ParseResult.GetValueForOption(matchedOption);
                }
                return null;
            }).ToArray();

            var result = method.Invoke(instance, args);
            
            if(result is null) return;
            if (result is Task task)
            {
                await task;
                return;
            }
        });
        return c;
    }

    private static Option? CreateOption(ParameterInfo parm)
    {
        var attribute = parm.GetCustomAttribute<OptionAttribute>();
        if (attribute is null)
            return null;

        var constructor = typeof(Option<>)
            .MakeGenericType(parm.ParameterType)
            .GetConstructor(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance,
                new[] { typeof(string), typeof(string) });
                
        var name = "--" + attribute.Name ?? parm.Name;
        var description = attribute.Description ?? parm.Name;
        
        var option = constructor?.Invoke([name, description]) as Option;
        if (option is null)
            return null;
        
        foreach (var shortName in attribute.ShortNames)
        {
            option.AddAlias("-"+shortName);            
        }
        
                    
        Console.WriteLine("Option:" + option);
        return option;
    }
}