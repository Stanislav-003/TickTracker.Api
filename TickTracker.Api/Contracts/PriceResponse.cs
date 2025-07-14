namespace TickTracker.Api.Contracts;

public record PriceResponse(
    string InstrumentId, 
    decimal? Ask, 
    decimal? Bid, 
    decimal? Last, 
    DateTime LastUpdate, 
    string Status);
