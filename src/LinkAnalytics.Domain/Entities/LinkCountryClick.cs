namespace LinkAnalytics.Domain.Entities;

public class LinkCountryClick
{
    public int LinkCountryClickId { get; set; }
    public string Country { get; set; } = "XX";   // كود ISO زي EG و SA، و XX = مجهولة
    public long Clicks { get; set; }

    public int LinkId { get; set; }
    public Link Link { get; set; } = null!;
}