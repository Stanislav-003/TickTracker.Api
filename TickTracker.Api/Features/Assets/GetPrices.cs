using Carter;
using MapsterMapper;
using MediatR;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;
using TickTracker.Api.Contracts;
using TickTracker.Api.Services;

namespace TickTracker.Api.Features.Assets;

public static class GetPrices
{
    public class Query : IRequest<InstrumentPrice>
    {
        public string InstrumentId { get; set; } = string.Empty;
    }

    internal sealed class Handler : IRequestHandler<Query, InstrumentPrice>
    {
        private readonly IPriceService _priceService;
        private readonly FintachartsWebSocketService _fintachartsWebSocketService;
        private readonly IMapper _mapper;

        public Handler(IPriceService priceService, FintachartsWebSocketService fintachartsWebSocketService, IMapper mapper)
        {
            _priceService = priceService;
            _fintachartsWebSocketService = fintachartsWebSocketService;
            _mapper = mapper;
        }

        public async Task<InstrumentPrice> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.InstrumentId))
            {
                throw new ArgumentException("InstrumentId is required");
            }

            await _fintachartsWebSocketService.WaitForConnectionAsync(TimeSpan.FromSeconds(5), cancellationToken);
            await _fintachartsWebSocketService.SendSubscriptionMessage(cancellationToken, request.InstrumentId);

            InstrumentPrice priceData;

            try
            {
                priceData = await _priceService.WaitForPriceAsync(request.InstrumentId, TimeSpan.FromSeconds(3));
            }
            catch (TaskCanceledException)
            {
                return new InstrumentPrice
                {
                    type = "Price timeout",
                    instrumentId = request.InstrumentId
                };
            }

            if (priceData == null)
            {
                return new InstrumentPrice
                {
                    type = "Price is unavailable",
                    instrumentId = request.InstrumentId,
                    provider = null,
                    last = null,
                };
            }

            return priceData;
        }
    }
}

public class GetPricesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/prices", async (string instrumentId, ISender mediatr, CancellationToken ct) =>
        {
            var query = new GetPrices.Query() { InstrumentId = instrumentId };

            var prices = await mediatr.Send(query, ct);

            return Results.Ok(prices);
        });
    }
}
