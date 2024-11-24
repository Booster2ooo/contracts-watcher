namespace ContractsWatcher.Services;

/// <summary>
/// The service used to provide the application addresses
/// </summary>
/// <param name="hostApplicationLifetime">The <see cref="IHostApplicationLifetime"/>.</param>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
public class AddressesProvider(
    IServer server,
    IHostApplicationLifetime hostApplicationLifetime
)
{
    public async Task<ICollection<string>> GetServerAddresses()
    {
        await WaitForApplicationStarted();
        return (server.Features.Get<IServerAddressesFeature>()?.Addresses) ?? throw new Exception("Unable to get the application addresses");
    }

    public async Task<string> GetPreferredAddress(string protocol)
    {
        var addresses = await GetServerAddresses();
        return addresses.FirstOrDefault(address => address.StartsWith(protocol)) ?? addresses.First();
    }

    private Task WaitForApplicationStarted()
    {
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        hostApplicationLifetime.ApplicationStarted.Register(() => completionSource.TrySetResult());
        return completionSource.Task;
    }
}
