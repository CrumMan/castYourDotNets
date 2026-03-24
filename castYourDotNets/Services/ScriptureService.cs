using castYourDotNets.Models;

namespace castYourDotNets.Services;

public class ScriptureService
{
    private readonly List<Scripture> scriptures =
    [
        new Scripture
        {
            Reference = "Mosiah 2:17",
            Text = "When ye are in the service of your fellow beings ye are only in the service of your God.",
            Topic = "Service"
        }
    ];

    public IReadOnlyList<Scripture> GetAll()
    {
        return scriptures
            .OrderBy(s => s.Reference)
            .ToList();
    }

    public Scripture? GetById(Guid id)
    {
        return scriptures.FirstOrDefault(s => s.Id == id);
    }

    public Scripture? GetNextPracticeTarget()
    {
        return scriptures
            .Where(s => !s.IsMemorized)
            .OrderBy(s => s.LastPracticedAtUtc ?? DateTime.MinValue)
            .ThenBy(s => s.Reference)
            .FirstOrDefault();
    }

    public bool RecordPractice(Guid id, bool succeeded)
    {
        var scripture = GetById(id);
        if (scripture is null)
        {
            return false;
        }

        scripture.PracticeCount += 1;

        var now = DateTime.UtcNow;
        if (scripture.LastPracticedAtUtc.HasValue)
        {
            var gap = (now.Date - scripture.LastPracticedAtUtc.Value.Date).Days;
            if (succeeded)
            {
                scripture.CurrentStreakDays = gap == 1 ? scripture.CurrentStreakDays + 1 : 1;
            }
            else
            {
                scripture.CurrentStreakDays = 0;
            }
        }
        else
        {
            scripture.CurrentStreakDays = succeeded ? 1 : 0;
        }

        scripture.LastPracticedAtUtc = now;
        return true;
    }

    public bool SetMemorized(Guid id, bool isMemorized)
    {
        var scripture = GetById(id);
        if (scripture is null)
        {
            return false;
        }

        scripture.IsMemorized = isMemorized;
        scripture.MemorizedAtUtc = isMemorized ? DateTime.UtcNow : null;
        return true;
    }

    public void Add(Scripture scripture)
    {
        scripture.Id = Guid.NewGuid();
        scripture.CreatedAtUtc = DateTime.UtcNow;
        scriptures.Add(scripture);
    }

    public bool Update(Scripture updatedScripture)
    {
        var existing = scriptures.FirstOrDefault(s => s.Id == updatedScripture.Id);
        if (existing is null)
        {
            return false;
        }

        existing.Reference = updatedScripture.Reference;
        existing.Text = updatedScripture.Text;
        existing.Topic = updatedScripture.Topic;
        existing.IsMemorized = updatedScripture.IsMemorized;
        existing.PracticeCount = updatedScripture.PracticeCount;
        existing.CurrentStreakDays = updatedScripture.CurrentStreakDays;
        existing.LastPracticedAtUtc = updatedScripture.LastPracticedAtUtc;
        existing.MemorizedAtUtc = updatedScripture.MemorizedAtUtc;
        return true;
    }

    public bool Delete(Guid id)
    {
        var existing = scriptures.FirstOrDefault(s => s.Id == id);
        if (existing is null)
        {
            return false;
        }

        scriptures.Remove(existing);
        return true;
    }
}
