namespace ConfigurationManager;

/// <summary>
/// Manages environment variables and paths
/// </summary>
class EnvironmentManager
{
    public string ScriptDir { get; }
    public string ConfDir { get; }
    public string LibDir { get; }
    public bool Debug { get; }
    public Dictionary<string, string> EnvironmentVariables { get; }
        
    public EnvironmentManager(CommandLineOptions options)
    {
        ScriptDir = Directory.GetCurrentDirectory();
        ConfDir = Path.Combine(ScriptDir, "conf");
        LibDir = Path.Combine(ScriptDir, "lib");
        Debug = options.Debug;
            
        // Setup environment variables for shell scripts
        EnvironmentVariables = new Dictionary<string, string>
        {
            ["CONF_DIR"] = ConfDir,
            ["LIB_DIR"] = LibDir,
            ["DEBUG"] = Debug ? "yes" : "",
            ["HOST"] = Environment.MachineName
        };
    }
        
    public void SetProfileEnvironment(Profile profile)
    {
        EnvironmentVariables["PROFILE_NAME"] = profile.Name;
        EnvironmentVariables["PROFILE_DESC"] = profile.Description;
    }
}