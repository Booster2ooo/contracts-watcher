using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ContractsWatcher.Controllers;

/// <summary>
/// API controller for managing contract-related operations.
/// </summary>
/// <param name="logger">The service used to log messages.</param>
/// <param name="hubContext">The <see cref="IHubContext{THub}"/> used for contracts.</param>
[ApiController]
[Route("api/[controller]")]
public class ContractsController(
    ILogger<ContractsController> logger,
    IHubContext<ContractsHub> hubContext
) : ControllerBase
{
    /// <summary>
    /// Handles the HTTP POST request to broadcast a new contract to all connected clients.
    /// </summary>
    /// <param name="contract">The contract information sent in the request body.</param>
    /// <returns>An IActionResult indicating the operation's outcome.</returns>
    [HttpPost(Name = "PostContract")]
    public async Task<IActionResult> NewContract([FromBody] string contract)
    {
        logger.LogTrace("Server received new contract '{contract}'", contract);
        await hubContext.Clients.All.SendAsync("NewContract", contract);
        return Ok();
    }
}
