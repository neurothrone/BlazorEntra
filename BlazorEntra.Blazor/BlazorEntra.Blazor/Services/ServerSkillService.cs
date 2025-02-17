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

    public Task<IEnumerable<Skill>> GetSkillsFromApiAsync()
    {
        return GetSkillsAsync("FromBlazorServerToWebAPI");
    }

    public Task<IEnumerable<Skill>> GetSkillsFromServerAsync()
    {
        // return GetSkillsAsync("BlazorServer");
        return Task.FromResult(_skillRepository.GetSkills());
    }

    private async Task<IEnumerable<Skill>> GetSkillsAsync(string client)
    {
        var httpClient = _httpClientFactory.CreateClient(client);

        var response = await httpClient.SendAsync(
            new HttpRequestMessage(
                HttpMethod.Get,
                client == "FromBlazorServerToWebAPI"
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