using System.Text.Json;
using BlazorEntra.Shared.Models;
using BlazorEntra.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorEntra.Blazor.Client.Services;

public class WasmSkillService : ISkillService
{
    // private readonly HttpClient _apiHttpClient;
    private readonly HttpClient _blazorServerHttpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public WasmSkillService(
        // [FromKeyedServices("FromBlazorWasmToWebAPI")]
        // HttpClient apiHttpClient,
        [FromKeyedServices("FromBlazorWasmToBlazorServerAPI")]
        HttpClient blazorServerHttpClient)
    {
        // _apiHttpClient = apiHttpClient;
        _blazorServerHttpClient = blazorServerHttpClient;
    }

    public async Task<IEnumerable<Skill>> GetSkillsFromApiAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "forward-to-web-api/skills");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await _blazorServerHttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Skill>>(
            await response.Content.ReadAsStreamAsync(),
            _jsonSerializerOptions,
            CancellationToken.None
        ) ?? [];
    }

    public async Task<IEnumerable<Skill>> GetSkillsFromServerAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "blazor/skills");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await _blazorServerHttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Skill>>(
            await response.Content.ReadAsStreamAsync(),
            _jsonSerializerOptions,
            CancellationToken.None
        ) ?? [];
    }
}