namespace ContractsWatcher.Models;

/// <summary>
/// Represents the <see cref="IConfiguration"/> for the <see cref="BrowserWindow"/> service
/// </summary>
public class BrowserOptions
{
    /// <summary>
    /// The configuration section's name
    /// </summary>
    public const string Position = "Browser";

    /// <summary>
    /// Gets/sets the path where browser is installed
    /// </summary>
    public string Executable { get; set; } = String.Empty;

    /// <summary>
    /// Gets/sets the port to attach the remote debugger to
    /// </summary>
    public int DebuggerPort { get; set; } = 16662;
}
