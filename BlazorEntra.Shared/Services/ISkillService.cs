using BlazorEntra.Shared.Models;

namespace BlazorEntra.Shared.Services;

public interface ISkillService
{
    Task<IEnumerable<Skill>> GetSkillsFromApiAsync();
    Task<IEnumerable<Skill>> GetSkillsFromServerAsync();
}