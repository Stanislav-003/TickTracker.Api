using System.ComponentModel.DataAnnotations;

namespace TickTracker.Api.Entities;

public class Asset
{
    public Guid Id { get; private set; }
    public string Symbol { get; private set; }
    public string Kind { get; private set; }
    public string Description { get; private set; }
    public double TickSize { get; private set; }
    public string Currency { get; private set; }
    public string BaseCurrency { get; private set; }

    private Asset() { }

    private Asset(Guid id, string symbol, string kind, string description, double tickSize, string currency, string baseCurrency)
    {
        Id = id;
        Symbol = symbol;
        Kind = kind;
        Description = description;
        TickSize = tickSize;
        Currency = currency;
        BaseCurrency = baseCurrency;
    }

    public static Asset Create(Guid id, string symbol, string kind, string description, double tickSize, string currency, string baseCurrency)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));
        
        if (string.IsNullOrWhiteSpace(kind))
            throw new ArgumentException("Kind cannot be empty.", nameof(kind));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        
        if (tickSize <= 0)
            throw new ArgumentException("TickSize must be positive.", nameof(tickSize));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));
        
        if (string.IsNullOrWhiteSpace(baseCurrency))
            throw new ArgumentException("BaseCurrency cannot be empty.", nameof(baseCurrency));

        return new Asset(id, symbol, kind, description, tickSize, currency, baseCurrency);
    }
}
