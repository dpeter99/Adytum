using System.Text.RegularExpressions;
using ConfigurationManager.Utils;
using Microsoft.Extensions.Logging;

namespace Adytum.Services;

/// <summary>
/// Enumeration of supported operating system types
/// </summary>
public enum OSType
{
    Fedora,
    RHEL,
    Unknown
}

public record struct OSInfo(OSType Type, Dictionary<string, string> ReleaseInfo)
{
    //public OSType Type;
    public string Name => ReleaseInfo.ContainsKey("NAME") ? ReleaseInfo["NAME"] : string.Empty;
    public string Version => ReleaseInfo.ContainsKey("VERSION") ? ReleaseInfo["VERSION"] : string.Empty;
    public string PackageManager => GetPackageManager(Type);
    //public Dictionary<string, string> ReleaseInfo;
    
    /// <summary>
    /// Get the package manager name based on the OS type
    /// </summary>
    public static string GetPackageManager(OSType osType)
    {
        return osType switch
        {
            OSType.Fedora => "dnf",
            OSType.RHEL => "dnf",
            _ => throw new ArgumentOutOfRangeException(nameof(osType), osType, null)
        };
    }
}

/// <summary>
/// Interface for detecting operating system information
/// </summary>
public interface IOperatingSystemDetector
{
    OSInfo GetOSInfo();
}

/// <summary>
/// Detects Linux distribution information
/// </summary>
public class LinuxOperatingSystemDetector : IOperatingSystemDetector
{
    private readonly ILogger<LinuxOperatingSystemDetector> _logger;
    
    // Lazy-loaded distribution info
    private Lazy<OSInfo> _OSInfo;
    
    public LinuxOperatingSystemDetector(
        ILogger<LinuxOperatingSystemDetector> logger)
    {
        _logger = logger;
        
        // Initialize lazy loaders
        _OSInfo = new (LoadOSInfo);
    }

    public OSInfo GetOSInfo()
    {
        return _OSInfo.Value;
    }

    #region Private Methods
    
    /// <summary>
    /// Loads information from /etc/os-release
    /// </summary>
    private OSInfo LoadOSInfo()
    {
        var result = new Dictionary<string, string>();
        
        // Try to load from /etc/os-release first (standard location)
        TryParseOSReleaseFile("/etc/os-release", result);
        
        // If we couldn't find basic info, try alternate locations
        if (!result.ContainsKey("ID") && !result.ContainsKey("NAME"))
        {
            TryParseOSReleaseFile("/usr/lib/os-release", result);
        }

        var type = MapOSType(result);
        
        return new OSInfo(type, result);
    }
    
    /// <summary>
    /// Parse the os-release file into a dictionary
    /// </summary>
    private void TryParseOSReleaseFile(string filePath, Dictionary<string, string> result)
    {
        if (!File.Exists(filePath))
            return;
            
        try
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;
                    
                var parts = line.Split('=', 2);
                if (parts.Length != 2)
                    continue;
                    
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                
                // Remove quotes if present
                if (value.StartsWith('"') && value.EndsWith('"'))
                    value = value.Substring(1, value.Length - 2);
                
                result[key] = value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing OS release file {FilePath}", filePath);
        }
    }
    
    /// <summary>
    /// Internal method to detect OS type from collected information
    /// </summary>
    private static OSType MapOSType(Dictionary<string, string> info)
    {
        // Check for ID field which is the primary identifier
        if (info.TryGetValue("ID", out var id))
        {
            return id.ToLowerInvariant() switch
            {
                "fedora" => OSType.Fedora,
                "rhel" => OSType.RHEL,
                _ => OSType.Unknown
            };
        }
        
        return OSType.Unknown;
    }
    
    #endregion
}