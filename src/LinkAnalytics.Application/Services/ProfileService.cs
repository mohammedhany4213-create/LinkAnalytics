using LinkAnalytics.Application.DTOs.Links;
using LinkAnalytics.Application.DTOs.Profiles;
using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Domain.Entities;

namespace LinkAnalytics.Application.Services;

public sealed class ProfileService(ILinkAnalyticsRepository repository) : IProfileService
{
    public async Task<ProfileResponse?> CreateAsync(
        CreateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0 ||
            string.IsNullOrWhiteSpace(request.Slug) ||
            string.IsNullOrWhiteSpace(request.Title) ||
            !await repository.UserExistsAsync(request.UserId, cancellationToken))
        {
            return null;
        }

        var slug = request.Slug.Trim().ToLowerInvariant();

        if (await repository.SlugExistsAsync(slug, cancellationToken))
            return null;

        var profile = new Profile
        {
            UserId = request.UserId,
            Slug = slug,
            Title = request.Title.Trim(),
            Bio = request.Bio?.Trim() ?? string.Empty
        };

        await repository.AddProfileAsync(profile, cancellationToken);

        return MapToResponse(profile);
    }

    public async Task<ProfileResponse?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var normalizedSlug = slug.Trim().ToLowerInvariant();

        await repository.IncrementProfileViewsAsync(normalizedSlug, cancellationToken);

        var profile = await repository.GetProfileBySlugAsync(
            normalizedSlug,
            cancellationToken);

        return profile is null ? null : MapToResponse(profile);
    }

    private static ProfileResponse MapToResponse(Profile profile) =>
        new(
            profile.ProfileId,
            profile.Slug,
            profile.Title,
            profile.Bio,
            profile.ViewsCount,
            profile.Links
                .OrderBy(link => link.Order)
                .Select(link => new LinkResponse(
                    link.LinkId,
                    link.Title,
                    link.Url,
                    link.Order))
                .ToArray());
}
