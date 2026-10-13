namespace LinkAnalytics.Application.DTOs.Links;

public sealed record CreateLinkRequest(
    int ProfileId,
    int UserId,
    string Title,
    string Url);
