using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Adytum.Services;
using Adytum.Services.Installer.Package;
using Adytum.Services.Installer.Repository;
using ConfigurationManager.Commands;
using ConfigurationManager.ConsoleApp;
using ConfigurationManager.Installer.Repository;
using ConfigurationManager.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.Spectre;

namespace ConfigurationManager
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Spectre()
                .CreateLogger();

            var builder = ConsoleApp.ConsoleApp.CreateHostBuilder(args, new HostApplicationBuilderSettings()
            {
                DisableDefaults = true,
            });

            builder.Services.AddSingleton<EnvironmentManager>();
            builder.Services.AddSingleton(typeof(ProfileManager));

            builder.Services.AddScoped<IPackageManagerFactory, PackageManagerFactory>();
            builder.Services.AddKeyedScoped<IPackageManager, DnfPackageManager>(nameof(DnfPackageManager));
            
            builder.Services.AddScoped<IRepositoryManagerFactory, RepositoryManagerFactory>();
            builder.Services.AddKeyedScoped<IRepositoryManager, FedoraRepositoryManager>(nameof(FedoraRepositoryManager));
            
            builder.Services.AddScoped<IOperatingSystemDetector, LinuxOperatingSystemDetector>();
            
            builder.Services.AddScoped<CliWrapper>();
            
            builder.AddGlobalOptions<GlobalOptions>(args);

            builder.Services.AddSerilog(
                (services, config) =>
                {
                    config.WriteTo.Spectre();

                    var opts = services.GetRequiredService<IOptions<GlobalOptions>>();
                    if (opts.Value.Debug)
                    {
                        config.MinimumLevel.Debug();
                    }
                    else
                    {
                        config.MinimumLevel.Information();
                    }
                });
            
            builder.AddCommand<SetupCommand>();
            
            var host = builder.Build();
            
            
            await host.Start(args);
        }
    }

    public class GlobalOptions
    {
        [Option('d')]
        public bool Debug { get; set; }
        [Option("dry")]
        public bool DryRun { get; set; }
        
        public bool Verbose { get; set; }
        public bool Quiet { get; set; }
        public bool Experimental { get; set; }
    }
    
    public class TestExperiment{}
}