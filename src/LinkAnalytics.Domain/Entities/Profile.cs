namespace LinkAnalytics.Domain.Entities;

public class Profile
{
    public int ProfileId {get; set;}
    public string Title {get; set;} = string.Empty ;
    public string Slug {get; set;} = string.Empty ;
    public string Bio {get; set;} = string.Empty ;
    public int UserId {get; set;}
    public User User {get; set;} = null! ;
}