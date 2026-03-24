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
