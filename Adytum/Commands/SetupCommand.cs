using System.CommandLine;
using Cocona;
using ConfigurationManager.ConsoleApp;
using Microsoft.Extensions.Configuration;

namespace ConfigurationManager.Commands;

public class SetupCommand
{
    ProfileManager _profileManager;

    public SetupCommand(ProfileManager profileManager)
    {
        _profileManager = profileManager;
    }


    [Command("setup",Description = "Sets up a new profile")]
    public async Task<int> Run (
        [Option("debug", ['d'], Description = "Enable debug output")] bool debug, 
        [Option("profile", ['p'], Description = "Specify the profile to use")] string? profileName
        )
    {
        var options = new CommandLineOptions
        {
            Debug = debug,
            Profile = profileName
        };

        // Setup environment
        var environment = new EnvironmentManager(options);

        // Show banner
        UI.ShowBanner("Profile Setup & Configuration");

        try
        {
            // Load profile
            var profileManager = new ProfileManager(environment);
            var profile = await profileManager.LoadProfileAsync(options.Profile);

            // Process the profile
            //var executor = new ModuleExecutor(environment, profile);
            //await executor.ProcessModulesAsync();

            // Complete
            UI.Section($"Setup complete for profile: {profile.Name}");
            UI.ShowSuccess("All tasks completed successfully!");
        }
        catch (Exception ex)
        {
            UI.Error($"Setup failed: {ex.Message}");
            return 1;
        }
            
        return 0;
    }
    
    public void Test()
    {
        Console.WriteLine("Test");
    }
}