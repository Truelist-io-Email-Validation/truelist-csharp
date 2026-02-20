#if NET6_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;

namespace Truelist;

/// <summary>
/// Extension methods for registering TruelistClient with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a singleton TruelistClient to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">Your Truelist API key.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTruelist(this IServiceCollection services, string apiKey)
    {
        return AddTruelist(services, apiKey, new TruelistOptions());
    }

    /// <summary>
    /// Adds a singleton TruelistClient to the service collection with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">Your Truelist API key.</param>
    /// <param name="options">Configuration options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTruelist(this IServiceCollection services, string apiKey, TruelistOptions options)
    {
        services.AddHttpClient("Truelist", client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("truelist-csharp/0.1.0");
        });

        services.AddSingleton(options);
        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient("Truelist");
            return new TruelistClient(apiKey, options, httpClient);
        });

        return services;
    }

    /// <summary>
    /// Adds a singleton TruelistClient to the service collection with a configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">Your Truelist API key.</param>
    /// <param name="configure">An action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTruelist(this IServiceCollection services, string apiKey, Action<TruelistOptions> configure)
    {
        var options = new TruelistOptions();
        configure(options);
        return AddTruelist(services, apiKey, options);
    }
}
#endif
