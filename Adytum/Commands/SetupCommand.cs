using ConfigurationManager.ConsoleApp;
using Microsoft.Extensions.Configuration;

namespace ConfigurationManager.Commands;

public class SetupCommand(ProfileManager profileManager, IPackageManagerFactory factory)
{
    ProfileManager _profileManager = profileManager;


    [Command("setup",Description = "Sets up a new profile")]
    public async Task<int> Run (
        [Option("debug", ['d'], Description = "Enable debug output")] bool debug, 
        [Option("profile", ['p'], Description = "Specify the profile to use")] string profileName
        )
    {
        
        UI.ShowBanner("Adytum Setup");
        
        var profile = await _profileManager.LoadProfileAsync(profileName);

        await factory.Create().InstallPackagesAsync(profile.Packages.Install);
        
        return 0;
    }
}