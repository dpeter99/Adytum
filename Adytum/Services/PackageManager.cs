using CliWrap;

namespace ConfigurationManager;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Manages package installation and COPR repositories
/// </summary>
class PackageManager
{
    private readonly EnvironmentManager _env;

    public PackageManager(EnvironmentManager env)
    {
        _env = env;
    }

    /// <summary>
    /// Run dnf copr enable command
    /// </summary>
    private async Task<bool> EnableDnfCoprAsync(string repo)
    {
        var result = await Cli.Wrap("dnf")
            .WithArguments(["dnf", "enable", "-y", repo])
            .ExecuteAsync();
        
        return result.ExitCode == 0;
    }

    /// <summary>
    /// Run dnf install command
    /// </summary>
    private async Task<bool> RunDnfInstallAsync(List<string> packages)
    {
        // Create a temporary file with list of packages
        var packageList = string.Join(" ", packages);
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, packageList);

        //UI.Spinner("Installing packages");

        var startInfo = new ProcessStartInfo
        {
            FileName = "sudo",
            Arguments = $"dnf install -y {packageList}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        var outputTask = new TaskCompletionSource<string>();
        var errorTask = new TaskCompletionSource<string>();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data == null)
            {
                outputTask.SetResult(outputTask.Task.Result ?? string.Empty);
            }
            else
            {
                Console.WriteLine(args.Data);
                outputTask.SetResult((outputTask.Task.Result ?? string.Empty) + args.Data + Environment.NewLine);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data == null)
            {
                errorTask.SetResult(errorTask.Task.Result ?? string.Empty);
            }
            else
            {
                Console.Error.WriteLine(args.Data);
                errorTask.SetResult((errorTask.Task.Result ?? string.Empty) + args.Data + Environment.NewLine);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        try
        {
            File.Delete(tempFile);
        }
        catch
        {
            // Ignore cleanup errors
        }

        return process.ExitCode == 0;
    }
}