namespace LinkAnalytics.Domain.Entities;

public class Profile
{
    public int ProfileId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public long ViewsCount { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Link> Links { get; set; } = new List<Link>();
}