using System.Diagnostics;
using System.Text;

namespace ConfigurationManager;

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