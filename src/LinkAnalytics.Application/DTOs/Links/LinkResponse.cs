namespace LinkAnalytics.Application.DTOs.Links;

public sealed record LinkResponse(
    int LinkId,
    string Title,
    string Url,
    int Order);
