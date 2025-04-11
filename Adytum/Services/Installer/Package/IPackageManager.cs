namespace Adytum.Services.Installer.Package;

/// <summary>
/// Abstract package manager interface to support multiple distros
/// </summary>
public interface IPackageManager
{
    Task<bool> InstallPackagesAsync(IEnumerable<string> packages);
    Task<bool> EnableRepositoryAsync(string repository);
}