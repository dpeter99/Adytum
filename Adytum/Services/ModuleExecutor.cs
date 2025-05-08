using System.Diagnostics;
using System.Text;
using Adytum.Model;
using ConfigurationManager;
using ConfigurationManager.Utils;
using Microsoft.Extensions.Logging;

namespace Adytum.Services;

/// <summary>
/// Executes modules based on profile configuration
/// </summary>
public class ModuleExecutor(EnvironmentManager env, ILogger<ModuleExecutor> logger, CliWrapper cli)
{
    public async Task ProcessModulesAsync(Profile profile)
    {
        UI.Section("Processing modules");
            
        if (profile.Modules.Enabled.Count == 0)
        {
            UI.Info("No modules specified");
            return;
        }
        
        var modules = FindModules(profile);
        
        // Sort modules by priority (numeric prefix)
        var sortedModules = modules.OrderBy(m => m.Priority).ToList();
        
        logger.LogDebug("Modules will be executed in this order: {ModuleOrder}", 
            string.Join(", ", sortedModules.Select(m => m.Id)));
            
        foreach (var module in sortedModules)
        {
            UI.Info($"Installing module: {module.Id}");
                
            UI.ModuleHeader(module.Id);
                
            try
            {
                var result = await ExecuteShellScriptAsync(module.ScriptFile);
                    
                if (result)
                {
                    UI.Task($"Successfully installed module: {module.Id}");
                }
                else
                {
                    UI.Warning($"Module installation may have issues: {module.Id}");
                }
                    
                UI.Separator();
            }
            catch (Exception ex)
            {
                UI.Error($"Error executing module {module.Id}: {ex.Message}");
                UI.Separator();
            }
        }
    }
    
    /// <summary>
    /// Find all modules specified in the profile
    /// </summary>
    private List<Module> FindModules(Profile profile)
    {
        var modules = new List<Module>();
        
        foreach (var moduleId in profile.Modules.Enabled)
        {
            var finalModuleId = moduleId;
            // Try to find the module script with exact ID
            FileInfo modulePath = new FileInfo(Path.Combine(env.ConfDir, "modules.d", $"{moduleId}.sh"));
            if (modulePath.Exists)
                goto found;
            
            // If exact ID not found, we might have a bare name like "nvm" instead of "110-nvm"
            // So search for matching module with a prefix
            var possibleFiles = Directory.GetFiles(env.ConfDir, $"modules.d/*-{moduleId}.sh")
                .OrderBy(f => f) // Sort by filename
                .Select(f => new FileInfo(f))
                .FirstOrDefault();
            
            if (possibleFiles is not null)
            {
                modulePath = possibleFiles;
                // Extract the actual ID from the filename
                finalModuleId = Path.GetFileNameWithoutExtension(modulePath.FullName);
            }
            
            
            found:
            if (modulePath.Exists)
            {
                modules.Add(new Module(finalModuleId, modulePath));
            }
            else
            {
                logger.LogWarning("Module script not found: {ModuleId}", moduleId);
                UI.Error($"Module installation script not found: {modulePath}");
            }
        }
        
        return modules;
    }
        
    private async Task<bool> ExecuteShellScriptAsync(FileInfo scriptPath)
    {
        var res = await cli.ExecuteCommandAsync(
            scriptPath,
            new List<string>());

        return res.Success;
    }
}