namespace TickTracker.Api.Options;

public class FintachartsWebSocketOptions
{
    public string WebSocketUrl { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string Provider { get; set; } = default!;
    public bool Subscribe { get; set; } 
    public string[] Kinds { get; set; } = Array.Empty<string>();
}