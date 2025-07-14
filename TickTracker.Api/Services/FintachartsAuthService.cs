using Microsoft.Extensions.Options;
using TickTracker.Api.Abstractions;
using TickTracker.Api.DTOs;
using TickTracker.Api.Options;

namespace TickTracker.Api.Services;

public class FintachartsAuthService : IFintachartsAuthService
{
    private readonly HttpClient _httpClient;
    private readonly FintachartsOptions _fintachartsOptions;
    private string? _accessToken;
    private DateTime _expiresAtUtc;

    public FintachartsAuthService(HttpClient httpClient, IOptions<FintachartsOptions> fintachartsOptions)
    {
        _httpClient = httpClient;
        _fintachartsOptions = fintachartsOptions.Value;
    }

    public async ValueTask<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_accessToken is { Length: > 0 } && _expiresAtUtc > DateTime.UtcNow.AddMinutes(1))
            return _accessToken;

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _fintachartsOptions.ClientId,
            ["username"] = _fintachartsOptions.Username,
            ["password"] = _fintachartsOptions.Password
        });

        var url = $"{_fintachartsOptions.BaseUri}/identity/realms/{_fintachartsOptions.Realm}/protocol/openid-connect/token";
        
        var response = await _httpClient.PostAsync(url, content, ct);

        var test = response.Content.ReadAsStringAsync(ct);

        response.EnsureSuccessStatusCode();

        var tokenDto = await response.Content.ReadFromJsonAsync<TokenDto>(cancellationToken: ct);

        if (tokenDto == null)
            throw new InvalidOperationException("Failed to get token from Fintacharts API.");

        _accessToken = tokenDto.Access_token;
        
        _expiresAtUtc = DateTime.UtcNow.AddSeconds(tokenDto.Expires_in);
        
        return _accessToken;
    }
}
