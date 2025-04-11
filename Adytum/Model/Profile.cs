using ConfigurationManager.Installer.Repository;

namespace ConfigurationManager;

/// <summary>
/// Represents a configuration profile
/// </summary>
public class Profile
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Inherit { get; set; }
    public ModuleConfig Modules { get; set; }
    public PackageConfig Packages { get; set; }
        
    public class ModuleConfig
    {
        public List<string> Enabled { get; set; } = new List<string>();
    }
        
    public class PackageConfig
    {
        public List<string> Copr { get; set; } = new List<string>();
        public List<string> Install { get; set; } = new List<string>();
        
        public List<RepositoryDescriptor> Repositories { get; set; } = new List<RepositoryDescriptor>();
    }
}

/// <summary>
/// Provides backward compatibility for profiles using legacy repository formats
/// </summary>
public static class ProfileConverter
{
    /// <summary>
    /// Convert legacy repository formats to the new RepositoryDescriptor format
    /// </summary>
    public static void ConvertLegacyRepositories(Profile profile)
    {
        // Convert COPR repositories
        if (profile.Packages.Copr.Any())
        {
            foreach (var coprRepo in profile.Packages.Copr)
            {
                // Skip if already converted
                if (profile.Packages.Repositories.Any(r => 
                        r.Type == "copr" && r.Name == coprRepo))
                    continue;
                    
                profile.Packages.Repositories.Add(new RepositoryDescriptor
                {
                    Name = coprRepo,
                    Type = "copr"
                });
            }
        }
    }
}