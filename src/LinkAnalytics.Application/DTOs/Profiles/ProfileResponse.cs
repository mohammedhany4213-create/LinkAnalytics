using LinkAnalytics.Application.DTOs.Links;

namespace LinkAnalytics.Application.DTOs.Profiles;

public sealed record ProfileResponse(
    int ProfileId,
    string Slug,
    string Title,
    string Bio,
    long ViewsCount,
    IReadOnlyCollection<LinkResponse> Links);
