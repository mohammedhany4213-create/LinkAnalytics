namespace LinkAnalytics.Application.DTOs.Profiles;

public sealed record CreateProfileRequest(
    int UserId,
    string Slug,
    string Title,
    string? Bio);
