using ConfigurationManager.ConsoleApp;
using Microsoft.Extensions.Configuration;

namespace ConfigurationManager.Commands;

public class SetupCommand(ProfileManager profileManager, UI ui)
{
    ProfileManager _profileManager = profileManager;


    [Command("setup",Description = "Sets up a new profile")]
    public async Task<int> Run (
        [Option("debug", ['d'], Description = "Enable debug output")] bool debug, 
        [Option("profile", ['p'], Description = "Specify the profile to use")] string profileName
        )
    {
        
        ui.ShowBanner("Adytum Setup");
        
        var profile = await _profileManager.LoadProfileAsync(profileName);
        
        return 0;
    }
    
    public void Test()
    {
        Console.WriteLine("Test");
    }
}