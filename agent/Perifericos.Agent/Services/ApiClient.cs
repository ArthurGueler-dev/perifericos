using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Perifericos.Agent.Models;

namespace Perifericos.Agent.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AgentOptions _options;

    public ApiClient(IHttpClientFactory httpClientFactory, IOptions<AgentOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient();
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl);
        if (!string.IsNullOrWhiteSpace(_options.AuthToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AuthToken);
        }
    }

    public async Task<AuthorizedDevicesResponse?> GetAuthorizedDevicesAsync(CancellationToken ct)
    {
        return await _httpClient.GetFromJsonAsync<AuthorizedDevicesResponse>($"/api/agents/{Uri.EscapeDataString(_options.PcIdentifier)}/authorized-devices", ct);
    }

    public async Task SendEventAsync(EventDto dto, CancellationToken ct)
    {
        using var resp = await _httpClient.PostAsJsonAsync("/api/events", dto, ct);
        resp.EnsureSuccessStatusCode();
    }
}

