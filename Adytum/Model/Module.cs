namespace Adytum.Model;

/// <summary>
/// Represents a basic module that can be executed
/// </summary>
public class Module
{
    /// <summary>
    /// The identifier of the module (e.g., "110-nvm")
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// The path to the script file
    /// </summary>
    public FileInfo ScriptFile { get; set; }
    
    public int Priority { get; set; }
    public string Name { get; set; }
    
    public Module(string moduleName, FileInfo scriptFile)
    {
        Id = moduleName;
        ScriptFile = scriptFile;
        
        Priority = GetPriority(Id);
        Name = GetName(Id);
    }
    
    /// <summary>
    /// Extract the numeric prefix from the module ID for sorting
    /// </summary>
    private static int GetPriority(string id)
    {
        if (string.IsNullOrEmpty(id)) return 999;
        
        var match = System.Text.RegularExpressions.Regex.Match(id, @"^(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int priority))
        {
            return priority;
        }
        
        return 999; // Default to lowest priority if no numeric prefix
    }
    
    /// <summary>
    /// Returns just the name part without the numeric prefix
    /// </summary>
    public static string GetName(string id)
    {
        if (string.IsNullOrEmpty(id)) return string.Empty;
        
        var match = System.Text.RegularExpressions.Regex.Match(id, @"^\d+-(.+)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        
        return id; // Return the full ID if no prefix is found
    }
}