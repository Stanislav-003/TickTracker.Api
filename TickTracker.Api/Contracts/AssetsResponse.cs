namespace TickTracker.Api.Contracts;

public record AssetsResponse(
    Guid Id,
    string Symbol,
    string Kind,
    string Description,
    double TickSize,
    string Currency,
    string BaseCurrency);
