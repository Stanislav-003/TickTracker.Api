using System.Collections.Concurrent;
using TickTracker.Api.Abstractions;
using TickTracker.Api.Contracts;

namespace TickTracker.Api.Services;

public class PriceService : IPriceService
{
    private readonly ConcurrentDictionary<string, List<InstrumentPrice>> _pricesHistory = new();

    const int MaxHistory = 10;

    public void UpdatePrice(InstrumentPrice priceData)
    {
        string instrumentId = priceData.instrumentId;

        if (_pricesHistory.TryGetValue(instrumentId, out var priceHistory))
        {
            lock (priceHistory)
            {
                priceHistory.Add(priceData);

                if (priceHistory.Count > MaxHistory)
                {
                    priceHistory.RemoveAt(0);
                }
            }
        }
        else
        {
            var newPriceList = new List<InstrumentPrice>();
            
            newPriceList.Add(priceData);

            _pricesHistory[instrumentId] = newPriceList;
        }
    }

    public InstrumentPrice? GetPriceById(string instrumentId)
    {
        if (_pricesHistory.TryGetValue(instrumentId, out var priceHistory))
        {
            lock (priceHistory)
            {
                return priceHistory.Count > 0 ? priceHistory[priceHistory.Count - 1] : null;
            }
        }

        return null;
    }
}