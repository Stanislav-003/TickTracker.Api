using TickTracker.Api.Contracts;

namespace TickTracker.Api.DTOs;

public record FintachartsWsPriceMessageDto(
    string type,
    string instrumentId,
    string provider,
    LastPrice? last,
    LastPrice? bid,
    LastPrice? ask
);

public record LastPriceDto(
    DateTime timestamp,
    decimal price,
    decimal volume,
    decimal? change,
    decimal? changePct
);
