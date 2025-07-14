namespace TickTracker.Api.Abstractions;

public interface IFintachartsAuthService
{
    ValueTask<string> GetTokenAsync(CancellationToken ct = default);
}
