using System.Text.Json.Serialization;

namespace ContractsWatcher.Models;

public record class ChromeSessionInfo(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("devtoolsFrontendUrl")] string DevtoolsFrontendUrl,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("webSocketDebuggerUrl")] string WebSocketDebuggerUrl
)
{
}
