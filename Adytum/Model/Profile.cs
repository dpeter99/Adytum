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
    }
}