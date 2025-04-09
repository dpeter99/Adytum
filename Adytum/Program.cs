using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ConfigurationManager.Commands;
using ConfigurationManager.ConsoleApp;
using Microsoft.Extensions.Configuration;
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
            
            builder.Services.AddSingleton<UI>();

            builder.Services.AddSingleton<EnvironmentManager>();
            builder.Services.AddSingleton(typeof(ProfileManager));

            builder.AddGlobalOptions<GlobalOptions>(args);

            var options = builder.Configuration.GetRequiredSection("GlobalOptions").Get<GlobalOptions>();
            if (options.Experimental)
            {
                builder.Services.AddSingleton<TestExperiment>();
            }
            
            builder.AddCommand<SetupCommand>();
            
            var host = builder.Build();
            
            
            await host.Start(args);
        }
    }

    public class GlobalOptions
    {
        [Option('d')]
        public bool Debug { get; set; }
        public bool Verbose { get; set; }
        public bool Quiet { get; set; }
        public bool Experimental { get; set; }
    }
    
    public class TestExperiment{}
}