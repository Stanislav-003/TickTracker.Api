using Carter;
using MapsterMapper;
using MediatR;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;
using TickTracker.Api.Abstractions;
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

            await _fintachartsWebSocketService.SendSubscriptionMessage(cancellationToken, request.InstrumentId);

            await Task.Delay(400);

            var priceData = _priceService.GetPriceById(request.InstrumentId);

            if (priceData == null)
            {
                throw new InvalidOperationException($"Price for instrument '{request.InstrumentId}' is unavailable or timed out.");
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
