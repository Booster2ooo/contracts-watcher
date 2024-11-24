using Microsoft.AspNetCore.SignalR;

namespace ContractsWatcher.Hubs;

/// <summary>
/// A SignalR hub for managing real-time communication of contract data.
/// </summary>
public class ContractsHub : Hub
{
    /// <summary>
    /// Sends a new contract message to all connected clients.
    /// </summary>
    /// <param name="contract">The contract information to be sent to clients.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendContract(string contract)
    {
        await Clients.All.SendAsync("NewContract", contract);
    }
}
