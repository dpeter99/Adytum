using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
    /// Executes modules based on profile configuration
    /// </summary>
    class ModuleExecutor
    {
        private readonly EnvironmentManager _env;
        private readonly Profile _profile;
        
        public ModuleExecutor(EnvironmentManager env, Profile profile)
        {
            _env = env;
            _profile = profile;
        }
        
        public async Task ProcessModulesAsync()
        {
            UI.Section("Processing modules");
            
            if (_profile.Modules?.Enabled == null || _profile.Modules.Enabled.Count == 0)
            {
                UI.Info("No modules specified");
                return;
            }
            
            foreach (var module in _profile.Modules.Enabled)
            {
                var modulePath = Path.Combine(_env.ConfDir, "modules.d", $"{module}.sh");
                
                // Check if module has a .install.sh extension
                if (!File.Exists(modulePath) && module.EndsWith(".install"))
                {
                    modulePath = Path.Combine(_env.ConfDir, "modules.d", $"{module}.sh");
                }
                
                UI.Info($"Installing module: {module}");
                
                if (!File.Exists(modulePath))
                {
                    UI.Error($"Module installation script not found: {modulePath}");
                    continue;
                }
                
                UI.ModuleHeader(module);
                
                try
                {
                    var result = await ExecuteShellScriptAsync(modulePath);
                    
                    if (result)
                    {
                        UI.Task($"Successfully installed module: {module}");
                    }
                    else
                    {
                        UI.Warning($"Module installation may have issues: {module}");
                    }
                    
                    UI.Separator();
                }
                catch (Exception ex)
                {
                    UI.Error($"Error executing module {module}: {ex.Message}");
                    UI.Separator();
                }
            }
        }
        
        private async Task<bool> ExecuteShellScriptAsync(string scriptPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = scriptPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            // Add environment variables
            foreach (var entry in _env.EnvironmentVariables)
            {
                startInfo.EnvironmentVariables[entry.Key] = entry.Value;
            }
            
            var process = new Process { StartInfo = startInfo };
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            
            process.OutputDataReceived += (sender, args) => 
            {
                if (args.Data != null)
                {
                    Console.WriteLine(args.Data);
                    outputBuilder.AppendLine(args.Data);
                }
            };
            
            process.ErrorDataReceived += (sender, args) => 
            {
                if (args.Data != null)
                {
                    Console.Error.WriteLine(args.Data);
                    errorBuilder.AppendLine(args.Data);
                }
            };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync();
            
            return process.ExitCode == 0;
        }
    }
    
    /// <summary>
    /// UI formatting helper methods (replacing utils.sh/gum functionality)
    /// </summary>
    static class UI
    {
        public static void ShowBanner(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(new string('=', Console.WindowWidth - 1));
            Console.WriteLine(CenterText(message));
            Console.WriteLine(new string('=', Console.WindowWidth - 1));
            Console.ResetColor();
            Console.WriteLine();
        }
        
        public static void ShowSuccess(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(CenterText(message));
            Console.ResetColor();
            Console.WriteLine();
        }
        
        public static void Section(string message)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
            Console.WriteLine();
        }
        
        public static void Task(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }
        
        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }
        
        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"! WARNING: {message}");
            Console.ResetColor();
        }
        
        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ ERROR: {message}");
            Console.ResetColor();
        }
        
        public static void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"DEBUG: {message}");
            Console.ResetColor();
        }
        
        public static void ModuleHeader(string moduleName)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('-', Console.WindowWidth - 1));
            Console.WriteLine(CenterText($"Module: {moduleName}"));
            Console.WriteLine(new string('-', Console.WindowWidth - 1));
            Console.ResetColor();
        }
        
        public static void Separator()
        {
            Console.WriteLine(new string('-', Console.WindowWidth - 1));
        }
        
        private static string CenterText(string text)
        {
            int padding = (Console.WindowWidth - text.Length) / 2;
            return text.PadLeft(padding + text.Length).PadRight(Console.WindowWidth - 1);
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