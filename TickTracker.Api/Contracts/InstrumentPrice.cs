namespace TickTracker.Api.Contracts;

public record InstrumentPrice(
    string type,
    string instrumentId,
    string provider,
    LastPrice? last,
    LastPrice? bid,
    LastPrice? ask
);

public record LastPrice(
    DateTime timestamp,
    decimal price,
    decimal volume,
    decimal? change,
    decimal? changePct
);
