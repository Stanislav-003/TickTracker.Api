namespace TickTracker.Api.DTOs;

public record AssetsDto(
    Guid Id, 
    string Symbol, 
    string Kind, 
    string Description, 
    double TickSize, 
    string Currency,
    string BaseCurrency);
