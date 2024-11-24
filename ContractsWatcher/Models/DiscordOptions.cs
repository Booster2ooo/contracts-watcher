namespace ContractsWatcher.Models;

/// <summary>
/// Represents the <see cref="IConfiguration"/> for the <see cref="DiscordLauncher"/> service
/// </summary>
public class DiscordOptions
{
    /// <summary>
    /// The configuration section's name
    /// </summary>
    public const string Position = "Discord";

    /// <summary>
    /// Gets/sets the path where Discord is installed
    /// </summary>
    public string Path { get; set; } = String.Empty;

    /// <summary>
    /// Gets/sets the full path to the JavaScript payload file to be injected in Discord
    /// </summary>
    public string JavaScriptPayload { get; set; } = String.Empty;

    /// <summary>
    /// Gets/sets the port to attach the remote debugger to
    /// </summary>
    public int DebuggerPort { get; set; } = 16661;
}
