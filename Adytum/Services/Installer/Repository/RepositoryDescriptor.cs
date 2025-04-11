namespace ConfigurationManager.Installer.Repository;

public class RepositoryDescriptor
{
    public string Name { get; set; }
    public string Type { get; set; } // "copr", "ppa", "aur", etc.
    public string Url { get; set; }
    public string Key { get; set; } // GPG key URL if needed
    public Dictionary<string, string> Metadata { get; set; } = new();
}