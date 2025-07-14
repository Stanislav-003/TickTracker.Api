namespace TickTracker.Api.Contracts;

public class InstrumentPrice
{
    public string type { get; set; } = string.Empty;
    public string instrumentId { get; set; } = string.Empty;
    public string provider { get; set; } = default!;
    public LastPrice? last { get; set; }
    public LastPrice? bid { get; set; }
    public LastPrice? ask { get; set; }
}

public class LastPrice
{
    public DateTime timestamp { get; set; }
    public decimal price { get; set; }
    public decimal volume { get; set; }
    public decimal? change { get; set; }
    public decimal? changePct { get; set; }
}
