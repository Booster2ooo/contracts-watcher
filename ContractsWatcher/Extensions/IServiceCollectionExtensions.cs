using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring the ContractsWatcher application.
/// </summary>
public static class ContractsWatcherCollectionExtensions
{
    /// <summary>
    /// Adds the configuration settings to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="config">The application configuration instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationConfig(
             this IServiceCollection services, IConfiguration config)
    {
        services
            .Configure<DiscordOptions>(config.GetSection(DiscordOptions.Position))
            .Configure<BrowserOptions>(config.GetSection(BrowserOptions.Position))
            ;
        return services;
    }

    /// <summary>
    /// Registers application services with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register the services with.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationServices(
         this IServiceCollection services)
    {
        services
            .AddHttpClient()
            .AddSingleton<AddressesProvider>()
            .AddScoped<RemoteDebugger>()
            .AddHostedService<DiscordLauncher>()
            .AddHostedService<BrowserManager>()
            ;
        return services;
    }
}
