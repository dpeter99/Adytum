namespace ConfigurationManager.Utils;

using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using System.Text;

/// <summary>
/// Wrapper for CLI operations that supports dry run mode
/// </summary>
public class CliWrapper(ILogger<CliWrapper> logger)
{

    /// <summary>
    /// Enable or disable dry run mode
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Execute a command with the specified arguments
    /// </summary>
    public async Task<CommandResult> ExecuteCommandAsync(
        string command, 
        IEnumerable<string> arguments, 
        bool requiresAdmin = false)
    {
        return await ExecuteCommandAsyncImpl(
            command,
            arguments,
            null,
            null,
            TimeSpan.FromHours(5),
            requiresAdmin
            );
    }

    /// <summary>
    /// Execute a command with a timeout
    /// </summary>
    public async Task<CommandResult> ExecuteCommandWithTimeoutAsync(
        string command,
        IEnumerable<string> arguments,
        TimeSpan timeout,
        bool requiresAdmin = false)
    {
        return await ExecuteCommandAsyncImpl(
            command, 
            arguments, 
            null, 
            null,
            timeout, 
            requiresAdmin
        );
    }

    /// <summary>
    /// Execute a command and stream its output
    /// </summary>
    public async Task<CommandResult> ExecuteCommandStreamingAsync(
        string command,
        IEnumerable<string> arguments,
        Action<string> stdoutCallback,
        Action<string> stderrCallback,
        bool requiresAdmin = false)
    {
        return await ExecuteCommandAsyncImpl(
            command, 
            arguments, 
            stdoutCallback, 
            stderrCallback, 
            TimeSpan.FromHours(5), 
            requiresAdmin
            );
    }
    
    internal async Task<CommandResult> ExecuteCommandAsyncImpl(
        string command,
        IEnumerable<string> arguments,
        Action<string>? stdoutCallback,
        Action<string>? stderrCallback,
        TimeSpan timeout,
        bool requiresAdmin = false)
    {
        var cmdString = command;
        var args = arguments.ToList();
        
        // If admin rights are required, prefix with sudo
        if (requiresAdmin && command != "sudo")
        {
            cmdString = "sudo";
            args.Insert(0, command);
        }

        // Build full command string for logging
        var fullCommand = $"{cmdString} {string.Join(" ", args)}";
        logger.LogDebug("Executing command: {Command}", fullCommand);
        
        // If in dry run mode return success and log the command
        if (DryRun)
        {
            // In dry run mode, just log the command and return success
            logger.LogInformation($"[DRY RUN] Would execute: {fullCommand}");
            
            var result = new CommandResult
            {
                Command = fullCommand,
                ExitCode = 0,
                StandardOutput = "[DRY RUN] Command simulated successfully",
                StandardError = string.Empty,
                Success = true
            };
            
            stdoutCallback?.Invoke(result.StandardOutput);
            return result;
        }
        
        var stdOutBuilder = new StringBuilder();
        var stdErrBuilder = new StringBuilder();
        
        try
        {
            // Execute with timeout
            var cts = new CancellationTokenSource(timeout);
            
            // Actually execute the command
            var result = await Cli.Wrap(cmdString)
                .WithArguments(args)
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(line => {
                    stdOutBuilder.AppendLine(line);
                    stdoutCallback?.Invoke(line);
                }))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(line => {
                    stdErrBuilder.AppendLine(line);
                    stderrCallback?.Invoke(line);
                }))
                .ExecuteBufferedAsync(cts.Token);
            
            return new CommandResult
            {
                Command = fullCommand,
                ExitCode = result.ExitCode,
                StandardOutput = result.StandardOutput,
                StandardError = result.StandardError,
                Success = result.ExitCode == 0
            };
        }
        catch (OperationCanceledException)
        {
            logger?.LogWarning("Command timed out after {Timeout}: {Command}", 
                timeout, fullCommand);
            
            return new CommandResult
            {
                Command = fullCommand,
                ExitCode = -1,
                StandardOutput = string.Empty,
                StandardError = $"Command timed out after {timeout}",
                Success = false,
                TimedOut = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Command execution failed: {Command}", fullCommand);
            
            return new CommandResult
            {
                Command = fullCommand,
                ExitCode = -1,
                StandardOutput = string.Empty,
                StandardError = ex.Message,
                Success = false,
                Exception = ex
            };
        }
        
    }
}

/// <summary>
/// Result of a command execution
/// </summary>
public class CommandResult
{
    /// <summary>
    /// The full command that was executed
    /// </summary>
    public string Command { get; set; } = string.Empty;
    
    /// <summary>
    /// Exit code returned by the command
    /// </summary>
    public int ExitCode { get; set; }
    
    /// <summary>
    /// Standard output from the command
    /// </summary>
    public string StandardOutput { get; set; } = string.Empty;
    
    /// <summary>
    /// Standard error from the command
    /// </summary>
    public string StandardError { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the command succeeded (exit code 0)
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Whether the command timed out
    /// </summary>
    public bool TimedOut { get; set; }
    
    /// <summary>
    /// Any exception that occurred during execution
    /// </summary>
    public Exception? Exception { get; set; }
}