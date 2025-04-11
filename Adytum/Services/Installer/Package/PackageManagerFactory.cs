using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adytum.Services.Installer.Package;

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