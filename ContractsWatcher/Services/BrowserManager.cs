namespace ContractsWatcher.Services;

/// <summary>
/// The background service used to manage the application view window
/// </summary>
/// <param name="logger">The service used to log messages.</param>
/// <param name="options">The configured <see cref="BrowserOptions"/>.</param>
/// <param name="addressesProvider">The <see cref="AddressesProvider"/>.</param>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
public class BrowserManager(
    ILogger<BrowserManager> logger,
    IOptions<BrowserOptions> options,
    AddressesProvider addressesProvider,
    IServiceProvider serviceProvider
) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting the application view...");
        var address = await addressesProvider.GetPreferredAddress("https://");
        var browserProccess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(Environment.ExpandEnvironmentVariables(options.Value.Executable)),
                UseShellExecute = false,

            }
        };
        //if (options.Value.Executable.Contains("firefox.exe"))
        //{
        //    browserProccess.StartInfo.ArgumentList.Add("-wait-for-browser"); // Windows only > https://wiki.mozilla.org/Firefox/CommandLineOptions#-wait-for-browser
        //    browserProccess.StartInfo.ArgumentList.Add("-new-window");
        //    browserProccess.StartInfo.ArgumentList.Add(address);
        //    do
        //    {
        //        logger.LogTrace("(Re)Launching browser window");
        //        browserProccess.Start();
        //        await browserProccess.WaitForExitAsync();
        //    } while (!stoppingToken.IsCancellationRequested);
        //}
        //else
        //{
        browserProccess.StartInfo.ArgumentList.Add($"--remote-debugging-port={options.Value.DebuggerPort}");
        browserProccess.StartInfo.ArgumentList.Add("--disable-default-apps");
        browserProccess.StartInfo.ArgumentList.Add("--disable-search-engine-choice-screen");
        browserProccess.StartInfo.ArgumentList.Add("--disable-sync");
        browserProccess.StartInfo.ArgumentList.Add("--no-first-run");
        browserProccess.StartInfo.ArgumentList.Add($"--user-data-dir={Environment.ExpandEnvironmentVariables("%TEMP%")}\\{Assembly.GetExecutingAssembly().GetName().Name}");
        browserProccess.StartInfo.ArgumentList.Add("--new-window");
        browserProccess.StartInfo.ArgumentList.Add(address);
        using var scope = serviceProvider.CreateScope();
        var remoteDebugger = scope.ServiceProvider.GetRequiredService<RemoteDebugger>();
        byte[] buffer = new byte[1024];
        do
        {
            logger.LogTrace("(Re)Launching browser window");
            browserProccess.Start();
            await Task.Delay(300, stoppingToken);
            var wsClient = await remoteDebugger.GetWebSocketDebugger(options.Value.DebuggerPort, stoppingToken);
            while (wsClient.State == WebSocketState.Open || wsClient.State == WebSocketState.CloseReceived)
            {
                try
                {
                    var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", stoppingToken);
                        wsClient.Dispose();
                        break;
                    }
                }
                catch
                {
                    // there is an exception... the ws is probably dead, isn't it ?
                    wsClient.Dispose();
                    break;
                }
            }
        } while (!stoppingToken.IsCancellationRequested);
        //}
    }
}
