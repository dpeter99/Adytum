using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cocona;
using ConfigurationManager.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfigurationManager
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            var builder = ConsoleApp.ConsoleApp.CreateHostBuilder(args, new HostApplicationBuilderSettings()
            {
                DisableDefaults = true,
            });
            
            builder.Services.AddSingleton(typeof(ProfileManager));
            
            builder.AddHelpCommand();
            builder.AddCommand<SetupCommand>();
            
            var host = builder.Build();
            
            
            await host.Start(args);
        }
    }

    public class CommandLineOptions
    {
        public bool Debug { get; set; }
        public string Profile { get; set; }
    }
}