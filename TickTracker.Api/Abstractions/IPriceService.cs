using TickTracker.Api.Contracts;

namespace TickTracker.Api.Abstractions;

public interface IPriceService
{
    void UpdatePrice(InstrumentPrice priceData);
    InstrumentPrice GetPriceById(string instrumentId);
}
