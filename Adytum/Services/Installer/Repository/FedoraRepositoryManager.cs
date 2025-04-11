using ConfigurationManager.Installer.Repository;
using ConfigurationManager.Utils;
using Microsoft.Extensions.Logging;

namespace Adytum.Services.Installer.Repository;

public class FedoraRepositoryManager(CliWrapper cli) : IRepositoryManager
{
    public async Task<bool> EnableRepositoryAsync(RepositoryDescriptor repo)
    {
        return repo.Type.ToLowerInvariant() switch
        {
            "copr" => await EnableCopr(repo),
            "rpm" => await AddRpmRepository(repo),
            _ => throw new NotSupportedException($"Repository type {repo.Type} not supported on Fedora")
        };
    }

    private async Task<bool> EnableCopr(RepositoryDescriptor repo)
    {
        var result = await cli.ExecuteCommandAsync("dnf", new[] { "copr", "enable", "-y", repo.Name }, requiresAdmin: true);
        return result.Success;
    }

    private async Task<bool> AddRpmRepository(RepositoryDescriptor repo)
    {
        // Import GPG key if provided
        if (!string.IsNullOrEmpty(repo.Key))
        {
            await cli.ExecuteCommandAsync("rpm", new[] { "--import", repo.Key }, requiresAdmin: true);
        }
        
        // Add the repository
        var result = await cli.ExecuteCommandAsync(
            "dnf", 
            new[] { "config-manager", "--add-repo", repo.Url },
            requiresAdmin: true);
        
        return result.Success;
    }
}