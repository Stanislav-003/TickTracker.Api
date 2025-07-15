using Carter;
using Microsoft.Extensions.Caching.Distributed;

namespace TickTracker.Api.Features.CacheDelete;

public class ClearCacheEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/cache/assets", async (IDistributedCache cache, CancellationToken ct) =>
        {
            const string key = "fintacharts:assets";
            await cache.RemoveAsync(key, ct);
            return Results.Ok($"Кеш по ключу '{key}' був видалений.");
        });
    }
}
