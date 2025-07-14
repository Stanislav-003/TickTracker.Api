using TickTracker.Api.Abstractions;
using TickTracker.Api.Options;
using TickTracker.Api.Services;

namespace TickTracker.Api.DependencyInjection;

public static class FintachartsExtensions
{
    public static IServiceCollection AddFintacharts(this IServiceCollection services, IConfiguration cfg)
    {
        services.Configure<FintachartsOptions>(cfg.GetSection("Fintacharts"));

        services.Configure<FintachartsWebSocketOptions>(cfg.GetSection("Fintacharts"));

        services.AddHttpClient<IFintachartsAuthService, FintachartsAuthService>();

        services.AddSingleton<FintachartsWebSocketService>();
        
        services.AddHostedService(sp => sp.GetRequiredService<FintachartsWebSocketService>());

        services.AddSingleton<IPriceService, PriceService>();

        services.AddHttpClient();

        services.AddHttpClient("Fintacharts", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddTypedClient<IFintachartsApiClient, FintachartsApiClient>();

        return services;
    }
}
