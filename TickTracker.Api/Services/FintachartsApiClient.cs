using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using TickTracker.Api.Abstractions;
using TickTracker.Api.DTOs;
using TickTracker.Api.Options;

namespace TickTracker.Api.Services;

public class FintachartsApiClient : IFintachartsApiClient
{
    private readonly IFintachartsAuthService _fintachartsAuthService;
    private readonly HttpClient _httpClient;
    private readonly FintachartsOptions _fintachartsOptions;

    public FintachartsApiClient(HttpClient httpClient, 
        IFintachartsAuthService fintachartsAuthService, 
        IOptions<FintachartsOptions> fintachartsOptions)
    {
        _httpClient = httpClient;
        _fintachartsAuthService = fintachartsAuthService;
        _fintachartsOptions = fintachartsOptions.Value;
    }

    public async Task<IReadOnlyList<AssetsDto>> GeaAllAssetsAsync(CancellationToken ct = default)
    {
        var token = await _fintachartsAuthService.GetTokenAsync(ct);

        var url = $"{_fintachartsOptions.BaseUri}/api/instruments/v1/instruments?provider={_fintachartsOptions.Provider}&kind={_fintachartsOptions.Kind}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
         
        var response = await _httpClient.SendAsync(request, ct);

        //var test = response.Content.ReadAsStringAsync(ct);

        response.EnsureSuccessStatusCode();

        var assetsResponseDto = await response.Content.ReadFromJsonAsync<AssetsResponseDto>(cancellationToken: ct);

        if (assetsResponseDto == null)
            throw new InvalidOperationException("Failed to parse response from Fintacharts API.");

        return assetsResponseDto.Data;
    }
}
