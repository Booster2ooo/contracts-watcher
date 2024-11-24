using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;

namespace ContractsWatcher.Services;

/// <summary>
/// The service used to get a remote debugger connection.
/// </summary>
/// <param name="logger">The service used to log messages.</param>
/// <param name="httpClientFactory">The factory used to build <see cref="HttpClient"/>s.</param>
public class RemoteDebugger(
    ILogger<RemoteDebugger> logger,
    IHttpClientFactory httpClientFactory
)
{
    /// <summary>
    /// Creates a connection to the debugger websocket.
    /// </summary>
    /// <param name="debuggerPort">The port of the debugger to connect to.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="ClientWebSocket"/> connected to the remote debugger.</returns>
    public async Task<ClientWebSocket> GetWebSocketDebugger(int debuggerPort, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting debugger on port {debuggerPort}", debuggerPort);
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("User-Agent", Assembly.GetExecutingAssembly().GetName().Name);
        await using Stream stream = await httpClient.GetStreamAsync($"http://localhost:{debuggerPort}/json", cancellationToken);
        var sessions = await JsonSerializer.DeserializeAsync<List<ChromeSessionInfo>>(stream, cancellationToken: cancellationToken);
        sessions ??= [];
        httpClient.Dispose();
        var wsClient = new ClientWebSocket();
        await wsClient.ConnectAsync(new Uri(sessions.First().WebSocketDebuggerUrl), cancellationToken);
        return wsClient;
    }
}
