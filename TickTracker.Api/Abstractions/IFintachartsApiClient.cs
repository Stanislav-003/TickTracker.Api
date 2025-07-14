using TickTracker.Api.Contracts;
using TickTracker.Api.DTOs;

namespace TickTracker.Api.Abstractions;

public interface IFintachartsApiClient
{
    Task<IReadOnlyList<AssetsDto>> GeaAllAssetsAsync(CancellationToken ct = default);
}
