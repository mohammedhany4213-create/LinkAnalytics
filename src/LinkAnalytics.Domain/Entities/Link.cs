namespace LinkAnalytics.Domain.Entities;

public class Link
{
    public int LinkId {get; set;}
    public string Title {get; set;} = string.Empty ;
    public string Url {get; set;} = string.Empty ;

    public int ProfileId {get; set;}
    public Profile Profile {get; set;} = null! ;
}