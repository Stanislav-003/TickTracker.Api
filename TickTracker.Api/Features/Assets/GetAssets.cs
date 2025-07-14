using Carter;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TickTracker.Api.Abstractions;
using TickTracker.Api.Contracts;
using TickTracker.Api.Database;
using TickTracker.Api.Entities;

namespace TickTracker.Api.Features.Assets;

public static class GetAssets
{
    public class Query : IRequest<IReadOnlyCollection<AssetsResponse>> { }

    internal sealed class Handler : IRequestHandler<Query, IReadOnlyCollection<AssetsResponse>>
    {
        private readonly IFintachartsApiClient _fintachartsApiClient;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;

        public Handler(IFintachartsApiClient fintachartsApiClient, IMapper mapper, ApplicationDbContext dbContext, IDistributedCache cache)
        {
            _fintachartsApiClient = fintachartsApiClient;
            _mapper = mapper;
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<IReadOnlyCollection<AssetsResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            const string key = "fintacharts:assets";
            const int cacheMinutes = 30;

            var cachedJson = await _cache.GetStringAsync(key);

            List<AssetsResponse>? assetsCache;

            if (!string.IsNullOrWhiteSpace(cachedJson))
            {
                assetsCache = JsonSerializer.Deserialize<List<AssetsResponse>>(cachedJson);

                if (assetsCache is not null)
                {
                    return assetsCache;
                }
            }

            var rawAssets = await _fintachartsApiClient.GeaAllAssetsAsync(cancellationToken);

            var assetEntities = rawAssets.Select(a =>
                Asset.Create(
                    id: a.Id,
                    symbol: a.Symbol,
                    kind: a.Kind,
                    description: a.Description,
                    tickSize: a.TickSize,
                    currency: a.Currency,
                    baseCurrency: a.BaseCurrency)).ToList();

            // Логіка у якій додаються дані, які існують в зовнішньому апі але не існують в базі
            var existingIds = await _dbContext.Assets
                .AsNoTracking()
                .Select(a => a.Id)
                .ToHashSetAsync(cancellationToken);

            var newAssets = assetEntities
                .Where(a => !existingIds.Contains(a.Id))
                .ToList();

            if (newAssets.Count != 0)
            {
                await _dbContext.Assets.AddRangeAsync(newAssets, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var response = _mapper.Map<List<AssetsResponse>>(assetEntities);

            await _cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheMinutes)
                },
                cancellationToken);

            return response;
        }
    }
}

public class GetAssetsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/assets", async (ISender mediatr) =>
        {
            var query = new GetAssets.Query { };

            var result = await mediatr.Send(query);

            if (result is null || result.Count == 0)
            {
                return Results.NotFound();
            }

            return Results.Ok(result);
        });
    }
}
