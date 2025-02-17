using BlazorEntra.Shared.Models;

namespace BlazorEntra.Blazor.Services;

public class SkillRepository
{
    public IEnumerable<Skill> GetSkills()
    {
        return
        [
            new Skill { Id = 1, Name = "Sarcasm" },
            new Skill { Id = 2, Name = "Irony" },
            new Skill { Id = 3, Name = "Satire" },
            new Skill { Id = 4, Name = "Parody" },
            new Skill { Id = 5, Name = "Wit" }
        ];
    }
}