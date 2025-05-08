using Adytum.Services;
using Adytum.Services.Installer.Package;
using Adytum.Services.Installer.Repository;
using ConfigurationManager.ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfigurationManager.Commands;

public class SetupCommand(
    ILogger<SetupCommand> logger,
    ProfileManager profileManager,
    IPackageManagerFactory packageMgrFactory,
    IOperatingSystemDetector osDetector,
    IRepositoryManagerFactory repositoryMgrFactory,
    ModuleExecutor moduleExecutor
    )
{
    [Command("setup",Description = "Sets up a new profile")]
    public async Task<int> Run (
        [Option("debug", ['d'], Description = "Enable debug output")] bool debug, 
        [Option("profile", ['p'], Description = "Specify the profile to use")] string profileName
        )
    {
        
        UI.ShowBanner("Adytum Setup");
        
        var osInfo = osDetector.GetOSInfo();
        UI.Section("System Information");
        UI.Info($"Operating System: {osInfo.Name} {osInfo.Version}");
        UI.Info($"System Type: {osInfo.Type}");
        UI.Info($"Package Manager: {osInfo.PackageManager}");
        
        var profile = await profileManager.LoadProfileAsync(profileName);

        await ProcessRepositoriesAsync(profile);
        
        await packageMgrFactory.Create().InstallPackagesAsync(profile.Packages.Install);

        await moduleExecutor.ProcessModulesAsync(profile);
        
        return 0;
    }
    
    /// <summary>
    /// Process repositories in the profile
    /// </summary>
    private async Task ProcessRepositoriesAsync(Profile profile)
    {
        UI.Section("Setting up repositories");

        if (!profile.Packages.Copr.Any())
        {
            UI.Info("No repositories specified in profile");
            return;
        }
        
        var repositoryManager = repositoryMgrFactory.Create();
        
        UI.SubTask($"Processing repositories ({profile.Packages.Repositories.Count})");

        foreach (var repo in profile.Packages.Repositories)
        {
            UI.SubTask($"Enabling {repo.Type} repository: {repo.Name}");

            try
            {
                await repositoryManager.EnableRepositoryAsync(repo);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to enable repository: {Name} ({Type})", repo.Name, repo.Type);
            }
        }
        
    }
}