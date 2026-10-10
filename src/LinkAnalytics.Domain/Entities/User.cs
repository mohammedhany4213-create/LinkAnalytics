namespace LinkAnalytics.Domain.Entities;

public class User
{
    public int UserId {get; set;}
    public string Name {get; set;} = string.Empty;
    public string UserName {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty ;
    public string PasswordHash {get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow ;
    public DateTime UpdatedAt {get; set;} 
    public bool IsActive {get; set;} = true ;
    public ICollection<Profile> Profiles {get; set;} = new List<Profile>();
}