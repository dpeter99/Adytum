using Microsoft.Extensions.Hosting;

namespace ConfigurationManager.ConsoleApp;

public class ConsoleApp
{
    public static ConsoleAppHostBuilder CreateHostBuilder(string[] args, HostApplicationBuilderSettings? settings = null)
    {
        return new(new(settings));
    }
}