namespace TickTracker.Api.DTOs;

public class FintachartsWsPriceMessageDto
{
    public string type { get; set; } = string.Empty;
    public string instrumentId { get; set; } = string.Empty;
    public string provider { get; set; } = default!;
    public LastPriceDto? last { get; set; }
    public LastPriceDto? bid { get; set; }
    public LastPriceDto? ask { get; set; }
}

public class LastPriceDto
{
    public DateTime timestamp { get; set; }
    public decimal price { get; set; }
    public decimal volume { get; set; }
    public decimal? change { get; set; }
    public decimal? changePct { get; set; }
}