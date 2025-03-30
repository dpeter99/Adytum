using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Parse command line arguments
            var options = ArgumentParser.Parse(args);
            
            // Setup environment
            var environment = new EnvironmentManager(options);
            
            // Show banner
            UI.ShowBanner("Profile Setup & Configuration");
            
            // Load profile
            var profileManager = new ProfileManager(environment);
            var profile = await profileManager.LoadProfileAsync(options.Profile);
            
            // Process the profile
            var executor = new ModuleExecutor(environment, profile);
            await executor.ProcessModulesAsync();
            
            // Complete
            UI.ShowSuccess("All tasks completed successfully!");
        }
    }

    /// <summary>
    /// Command line options parser
    /// </summary>
    class ArgumentParser
    {
        public static CommandLineOptions Parse(string[] args)
        {
            var options = new CommandLineOptions();
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-d":
                    case "--debug":
                        options.Debug = true;
                        break;
                    case "-p":
                    case "--profile":
                        if (i + 1 < args.Length)
                        {
                            options.Profile = args[++i];
                        }
                        break;
                    default:
                        UI.Warning($"Unknown option: {args[i]}");
                        break;
                }
            }
            
            return options;
        }
    }
    
    class CommandLineOptions
    {
        public bool Debug { get; set; }
        public string Profile { get; set; }
    }
}