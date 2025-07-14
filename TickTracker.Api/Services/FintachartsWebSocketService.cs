using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using TickTracker.Api.Options;
using TickTracker.Api.Abstractions;
using TickTracker.Api.Contracts;
using TickTracker.Api.DTOs;

namespace TickTracker.Api.Services;

public class FintachartsWebSocketService : BackgroundService
{
    private readonly IPriceService _priceService;
    private readonly IFintachartsAuthService _fintachartsAuthService;
    private readonly FintachartsWebSocketOptions _fintachartsWebSocketOptions;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger<FintachartsWebSocketService> _logger;
    private ClientWebSocket _webSocket;

    public FintachartsWebSocketService(
        IPriceService priceService,
        IOptions<FintachartsWebSocketOptions> fintachartsWebSocketOptions,
        IFintachartsAuthService fintachartsAuthService,
        ILogger<FintachartsWebSocketService> logger)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _priceService = priceService;
        _fintachartsAuthService = fintachartsAuthService;
        _fintachartsWebSocketOptions = fintachartsWebSocketOptions.Value;
        _logger = logger;
        _webSocket = null!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Fintacharts WebSocket service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndSubscribe(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WebSocket loop. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Fintacharts WebSocket service is stopping.");
    }

    private async Task ConnectAndSubscribe(CancellationToken cancellationToken)
    {
        _webSocket = new ClientWebSocket();

        string token;

        try
        {
            token = await _fintachartsAuthService.GetTokenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get authentication token.");
            return;
        }

        var url = $"{_fintachartsWebSocketOptions.WebSocketUrl}?token={token}";

        Uri uri = new(url);

        try
        {
            await _webSocket.ConnectAsync(uri, cancellationToken);

            await SendSubscriptionMessage(cancellationToken);

            await ListenForMessages(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect and subscribe to WebSocket.");
        }
        finally
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close connection", cancellationToken);
                _logger.LogInformation("WebSocket connection closed.");
            }

            _webSocket?.Dispose();
        }
    }

    public async Task SendSubscriptionMessage(CancellationToken cancellationToken, string? id = null)
    {
        try
        {
            if (_webSocket?.State != WebSocketState.Open)
            {
                _logger.LogWarning("Attempted to send message, but WebSocket is not connected.");
                return;
            }

            var subscriptionMessage = new
            {
                type = _fintachartsWebSocketOptions.Type,
                id = _fintachartsWebSocketOptions.Id,
                instrumentId = String.IsNullOrWhiteSpace(id) ? "ad9e5345-4c3b-41fc-9437-1d253f62db52" : id,
                provider = _fintachartsWebSocketOptions.Provider,
                subscribe = _fintachartsWebSocketOptions.Subscribe,
                kinds = _fintachartsWebSocketOptions.Kinds,
            };

            var json = JsonSerializer.Serialize(subscriptionMessage);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send subscription message.");
        }
    }

    private async Task ListenForMessages(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];

        while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    ProcessReceivedMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning("WebSocket close message received.");
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Listening cancelled.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while receiving WebSocket message.");
                break;
            }
        }
    }

    private void ProcessReceivedMessage(string message)
    {
        try
        {
            var fintaMessage = JsonSerializer.Deserialize<FintachartsWsPriceMessageDto>(message);

            if (fintaMessage == null)
            {
                _logger.LogWarning("Received malformed or incomplete WebSocket message: {Message}", message);
                return;
            }

            if (fintaMessage != null)
            {
                var instrumentPrice = new InstrumentPrice
                {
                    type = fintaMessage.type,
                    instrumentId = fintaMessage.instrumentId,
                    provider = fintaMessage.provider
                };

                if (fintaMessage.last != null)
                {
                    instrumentPrice.last = new LastPrice
                    {
                        timestamp = fintaMessage.last.timestamp,
                        price = fintaMessage.last.price,
                        volume = fintaMessage.last.volume,
                        change = fintaMessage.last.change,
                        changePct = fintaMessage.last.changePct
                    };
                }

                if (fintaMessage.bid != null)
                {
                    instrumentPrice.bid = new LastPrice
                    {
                        timestamp = fintaMessage.bid.timestamp,
                        price = fintaMessage.bid.price,
                        volume = fintaMessage.bid.volume,
                        change = fintaMessage.bid.change,
                        changePct = fintaMessage.bid.changePct
                    };
                }

                if (fintaMessage.ask != null)
                {
                    instrumentPrice.ask = new LastPrice
                    {
                        timestamp = fintaMessage.ask.timestamp,
                        price = fintaMessage.ask.price,
                        volume = fintaMessage.ask.volume,
                        change = fintaMessage.ask.change,
                        changePct = fintaMessage.ask.changePct
                    };
                }

                _priceService.UpdatePrice(instrumentPrice);
                _logger.LogDebug("Updated price for instrument {InstrumentId}", instrumentPrice.instrumentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process received WebSocket message: {Message}", message);
        }
    }

    public async Task WaitForConnectionAsync(TimeSpan timeout, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        
        while (_webSocket?.State != WebSocketState.Open && DateTime.UtcNow - start < timeout)
            await Task.Delay(50, ct);

        if (_webSocket?.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not connected.");
    }
}
