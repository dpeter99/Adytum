using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Runtime.InteropServices;
using ConfigurationManager.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConfigurationManager;

/// <summary>
/// Abstract package manager interface to support multiple distros
/// </summary>
public interface IPackageManager
{
    Task<bool> InstallPackagesAsync(IEnumerable<string> packages);
    Task<bool> EnableRepositoryAsync(string repository);
}

public enum PackageManagerType
{
    Debian,
    Fedora,
    Arch,
    Unknown
}

public interface IPackageManagerFactory
{
    IPackageManager Create();
}

/// <summary>
/// Factory to create the appropriate package manager based on the system
/// </summary>
public class PackageManagerFactory(
    ILogger<PackageManagerFactory> logger,
    IServiceProvider serviceProvider
    ) : IPackageManagerFactory
{
    public IPackageManager Create()
    {
        var type = PackageManagerType.Fedora;
        return type switch
        {
            PackageManagerType.Arch => throw new NotImplementedException(),
            PackageManagerType.Debian => throw new NotImplementedException(),
            PackageManagerType.Fedora => serviceProvider.GetRequiredKeyedService<IPackageManager>(
                nameof(DnfPackageManager)),
            PackageManagerType.Unknown => throw new PlatformNotSupportedException(
                "Current platform is not supported for package management"),
            _ => throw new NotImplementedException($"Package manager for {type} is not implemented")
        };
    }
}

/// <summary>
/// DNF package manager implementation for Fedora-based systems
/// </summary>
public class DnfPackageManager(
    CliWrapper cli,
    ILogger<DnfPackageManager> logger
    )
    : IPackageManager
{

    public async Task<bool> InstallPackagesAsync(IEnumerable<string> packages)
    {
        if (!packages.Any())
        {
            logger.LogInformation("No packages provided");
            return true;
        }

        var packageList = string.Join(" ", packages);
        logger.LogInformation("Installing the following packages: " + packageList);
        
        try
        {
            return await UI.SpinnerAsync("Installing packages", async (ctx) =>
            {
                var result = await cli.ExecuteCommandStreamingAsync(
                    "dnf", 
                    new[] { "install", "-y" }.Concat(packages),
                    UI.Info,
                    UI.Info,
                    requiresAdmin: true);
                
                if (result.Success)
                {
                    UI.Task("Successfully installed all packages");
                    return true;
                }
                else
                {
                    logger.LogWarning("DNF install error: {Error}", result.StandardError);
                    return false;
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to install packages: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EnableRepositoryAsync(string repository)
    {
        if (string.IsNullOrWhiteSpace(repository))
        {
            UI.Warning("No repository specified");
            return false;
        }

        UI.SubTask($"Enabling COPR repository: {repository}");
        
        try
        {
            return await UI.SpinnerAsync($"Enabling COPR repository: {repository}", async (_) =>
            {
                var result = await cli.ExecuteCommandAsync(
                    "dnf", 
                    new[] { "copr", "enable", "-y", repository },
                    requiresAdmin: true);
                
                if (result.Success)
                {
                    UI.Task($"Enabled COPR repository: {repository}");
                    return true;
                }
                else
                {
                    logger.LogWarning($"Failed to enable COPR repository: {repository} (Exit code: {result.ExitCode})");
                    return false;
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to enable repository: {ex.Message}");
            return false;
        }
    }


}