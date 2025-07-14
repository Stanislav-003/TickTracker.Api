using System.Collections.Concurrent;
using TickTracker.Api.Contracts;

namespace TickTracker.Api.Services;

public class PriceService : IPriceService
{
    private readonly ConcurrentDictionary<string, List<InstrumentPrice>> _pricesHistory = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<InstrumentPrice>> _waiters = new();

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

        if (_waiters.TryRemove(priceData.instrumentId, out var waiter))
            waiter.TrySetResult(priceData);
    }

    public InstrumentPrice GetPriceById(string instrumentId)
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

    public Task<InstrumentPrice> WaitForPriceAsync(string instrumentId, TimeSpan timeout)
    {
        if (GetPriceById(instrumentId) is { } p) return Task.FromResult(p);

        var tcs = _waiters.GetOrAdd(instrumentId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously));
        
        var cts = new CancellationTokenSource(timeout);
        
        cts.Token.Register(() => tcs.TrySetCanceled());
        
        return tcs.Task;
    }
}

public interface IPriceService
{
    void UpdatePrice(InstrumentPrice priceData);
    InstrumentPrice GetPriceById(string instrumentId);
    Task<InstrumentPrice> WaitForPriceAsync(string id, TimeSpan timeout);
}