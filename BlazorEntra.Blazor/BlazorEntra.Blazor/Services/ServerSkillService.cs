using System.Text.Json;
using BlazorEntra.Shared.Models;
using BlazorEntra.Shared.Services;

namespace BlazorEntra.Blazor.Services;

public class ServerSkillService : ISkillService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SkillRepository _skillRepository;

    public ServerSkillService(
        IHttpClientFactory httpClientFactory,
        SkillRepository skillRepository)
    {
        _httpClientFactory = httpClientFactory;
        _skillRepository = skillRepository;
    }

    public async Task<IEnumerable<Skill>> GetSkillsFromApiAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("FromBlazorServerToWebAPI");
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/skills");
        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Skill>>(
            await response.Content.ReadAsStreamAsync(),
            _jsonSerializerOptions,
            CancellationToken.None
        ) ?? [];
    }

    public Task<IEnumerable<Skill>> GetSkillsFromServerAsync()
    {
        return Task.FromResult(_skillRepository.GetSkills());
    }
}