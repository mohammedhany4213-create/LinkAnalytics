namespace LinkAnalytics.Application.DTOs.Links;

public sealed record CountryClickResponse(string Country, long Clicks);

public sealed record LinkAnalyticsResponse(
    int LinkId,
    string Title,
    string Url,
    long TotalClicks,
    IReadOnlyCollection<CountryClickResponse> CountryClicks,
    double ClickThroughRate);