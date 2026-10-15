namespace LinkAnalytics.Application.DTOs.Profiles;

public sealed record UpdateProfileRequest(string Slug, string Title, string? Bio);