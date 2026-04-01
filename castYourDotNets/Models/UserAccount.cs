namespace castYourDotNets.Models;

public sealed class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string NormalizedUsername { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public List<PageClass> PageClasses { get; } = [];

    /// <summary>
    /// User's scripture memorization progress and tracking
    /// </summary>
    public List<UserScripture> UserScriptures { get; } = [];
}