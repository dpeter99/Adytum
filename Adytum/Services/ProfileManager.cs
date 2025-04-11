using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigurationManager;

/// <summary>
/// Manages profile loading and inheritance
/// </summary>
public class ProfileManager(EnvironmentManager env)
{
    public async Task<Profile> LoadProfileAsync(string profileName)
    {
        UI.Section($"Loading profile: {profileName}");
            
        var profilePath = Path.Combine(env.ConfDir, "profiles.d", $"{profileName}.yaml");
            
        if (!File.Exists(profilePath))
        {
            UI.Error($"Profile not found: {profileName}");
            throw new FileNotFoundException($"Profile not found: {profileName}");
        }
            
        var profile = await LoadProfileWithInheritanceAsync(profilePath);
            
        // Convert legacy repository formats
        ProfileConverter.ConvertLegacyRepositories(profile);
        
        UI.Section($"Setting up profile: {profile.Name}");
        UI.Info($"Description: {profile.Description}");
        
        UI.Debug($"Effective profile content: {System.Text.Json.JsonSerializer.Serialize(profile, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
        
        env.SetProfileEnvironment(profile);
            
        return profile;
    }
        
    private async Task<Profile> LoadProfileWithInheritanceAsync(string profilePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
                
        var profileContent = await File.ReadAllTextAsync(profilePath);
        var profile = deserializer.Deserialize<Profile>(profileContent);
            
        // Handle inheritance
        if (!string.IsNullOrEmpty(profile.Inherit))
        {
            UI.Info($"Profile {profile.Name} inherits from: {profile.Inherit}");
                
            var parentPath = Path.Combine(env.ConfDir, "profiles.d", $"{profile.Inherit}.yaml");
                
            if (!File.Exists(parentPath))
            {
                UI.Error($"Parent profile not found: {profile.Inherit}");
                throw new FileNotFoundException($"Parent profile not found: {profile.Inherit}");
            }
                
            var parentProfile = await LoadProfileWithInheritanceAsync(parentPath);
                
            // Merge parent and child profiles
            profile = MergeProfiles(parentProfile, profile);
        }
            
        return profile;
    }
        
    private Profile MergeProfiles(Profile parent, Profile child)
    {
        // Create a new profile with inherited values
        var merged = new Profile
        {
            Name = child.Name,
            Description = child.Description,
            Inherit = child.Inherit,
            Modules = new Profile.ModuleConfig
            {
                Enabled = new List<string>(parent.Modules?.Enabled ?? new List<string>())
            },
            Packages = new Profile.PackageConfig
            {
                Copr = new List<string>(parent.Packages?.Copr ?? new List<string>()),
                Install = new List<string>(parent.Packages?.Install ?? new List<string>())
            }
        };
            
        // Add child values
        if (child.Modules?.Enabled != null)
        {
            merged.Modules.Enabled.AddRange(child.Modules.Enabled);
        }
            
        if (child.Packages?.Copr != null)
        {
            merged.Packages.Copr.AddRange(child.Packages.Copr);
        }
            
        if (child.Packages?.Install != null)
        {
            merged.Packages.Install.AddRange(child.Packages.Install);
        }
            
        return merged;
    }
}