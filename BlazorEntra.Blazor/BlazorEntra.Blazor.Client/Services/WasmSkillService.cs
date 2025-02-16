using System.Text.Json;
using BlazorEntra.Shared.Models;
using BlazorEntra.Shared.Services;

namespace BlazorEntra.Blazor.Client.Services;

public class WasmSkillService : ISkillService
{
    private readonly HttpClient _apiHttpClient;
    private readonly HttpClient _blazorServerHttpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public WasmSkillService(
        [FromKeyedServices("FromBlazorWasmToWebAPI")]
        HttpClient apiHttpClient,
        [FromKeyedServices("FromBlazorWasmToBlazorServerAPI")]
        HttpClient blazorServerHttpClient)
    {
        _apiHttpClient = apiHttpClient;
        _blazorServerHttpClient = blazorServerHttpClient;
    }

    public Task<IEnumerable<Skill>> GetSkillsFromApiAsync()
    {
        return GetSkillsAsync("FromBlazorWasmToWebAPI");
    }

    public Task<IEnumerable<Skill>> GetSkillsFromServerAsync()
    {
        return GetSkillsAsync("FromBlazorWasmToBlazorServerAPI");
    }

    private async Task<IEnumerable<Skill>> GetSkillsAsync(string client)
    {
        var response = await (client switch
        {
            "FromBlazorWasmToWebAPI" => _apiHttpClient,
            "FromBlazorWasmToBlazorServerAPI" => _blazorServerHttpClient,
            _ => throw new ArgumentException("Invalid client name.")
        }).SendAsync(
            new HttpRequestMessage(
                HttpMethod.Get,
                client == "FromBlazorWasmToWebAPI"
                    ? "api/skills"
                    : "blazor/skills"
            )
        );
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Skill>>(
            await response.Content.ReadAsStreamAsync(),
            _jsonSerializerOptions,
            CancellationToken.None
        ) ?? [];
    }
}