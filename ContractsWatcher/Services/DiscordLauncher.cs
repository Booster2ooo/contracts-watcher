using System.Text;
using System.Text.Json;

namespace ContractsWatcher.Services;

/// <summary>
/// The background service used to launch Discord and inject the JS payload
/// </summary>
/// <param name="logger">The service used to log messages.</param>
/// <param name="options">The configured <see cref="DiscordOptions"/>.</param>
/// <param name="addressesProvider">The <see cref="AddressesProvider"/>.</param>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
public class DiscordLauncher(
    ILogger<DiscordLauncher> logger,
    IOptions<DiscordOptions> options,
    AddressesProvider addressesProvider,
    IServiceProvider serviceProvider
) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DiscordLauncher starting...");
        logger.LogTrace("Shutting down existing Discord instances.");
        var clearProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = "taskkill",
            UseShellExecute = true,
            Arguments = "/f /im Discord.exe"
        }) ?? throw new Exception("Unable to kill existing Discord processes");
        await clearProcess.WaitForExitAsync(stoppingToken);
        logger.LogTrace("Starting Discord launcher.");
        var launcherProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = Path.Combine(Environment.ExpandEnvironmentVariables(options.Value.Path), "Update.exe"),
            Arguments = $"--processStart Discord.exe --process-start-args --remote-debugging-port={options.Value.DebuggerPort}",
            UseShellExecute = false
        }) ?? throw new Exception("Unable to start Discord process");
        await launcherProcess.WaitForExitAsync(stoppingToken);
        await Task.Delay(300, stoppingToken);
        logger.LogTrace("Building injection payload.");
        var scriptPath = Environment.ExpandEnvironmentVariables(options.Value.JavaScriptPayload.Replace("%CWD%", AppDomain.CurrentDomain.BaseDirectory));
        var serverAddress = (await addressesProvider.GetPreferredAddress("http://")).Replace("localhost", "127.0.0.1");
        var script = File.ReadAllText(scriptPath, Encoding.UTF8).Replace("%SERVER%", serverAddress);
        dynamic command = new
        {
            id = 1234,
            method = "Runtime.evaluate",
            @params = new
            {
                expression = script,
                objectGroup = Assembly.GetExecutingAssembly().GetName().Name,
                silent = true,
                returnByValue = false,
                userGesture = true
            }
        };
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(command);
        using var scope = serviceProvider.CreateScope();
        var remoteDebugger = scope.ServiceProvider.GetRequiredService<RemoteDebugger>();
        var wsClient = await remoteDebugger.GetWebSocketDebugger(options.Value.DebuggerPort, stoppingToken);
        logger.LogTrace("Injecting script");
        await wsClient.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, stoppingToken);
        //wsClient.Close();?
        wsClient.Dispose();
        logger.LogInformation("DiscordLauncher completed.");
    }
}
