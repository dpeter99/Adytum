using ConfigurationManager.Installer.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adytum.Services.Installer.Repository;

public interface IRepositoryManagerFactory
{
    IRepositoryManager Create();
}

public class RepositoryManagerFactory : IRepositoryManagerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RepositoryManagerFactory> _logger;
    private readonly IOperatingSystemDetector _osDetector;
    
    public RepositoryManagerFactory(
        IServiceProvider serviceProvider,
        ILogger<RepositoryManagerFactory> logger,
        IOperatingSystemDetector osDetector)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _osDetector = osDetector;
    }
    
    public IRepositoryManager Create()
    {
        var osType = _osDetector.GetOSInfo().Type;
        
        return osType switch
        {
            OSType.Fedora => _serviceProvider.GetRequiredKeyedService<IRepositoryManager>(
                nameof(FedoraRepositoryManager)),
            _ => throw new PlatformNotSupportedException($"No repository manager available for {osType}")
        };
    }
}