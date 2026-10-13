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
        if (!await repository.UserExistsAsync(request.UserId, cancellationToken))
            return null;

        var slug = request.Slug.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(slug) ||
            await repository.SlugExistsAsync(slug, cancellationToken))
            return null;

        var profile = new Profile
        {
            UserId = request.UserId,
            Slug = slug,
            Title = request.Title.Trim(),
            Bio = request.Bio?.Trim() ?? string.Empty
        };

        await repository.AddProfileAsync(profile, cancellationToken);

        return new ProfileResponse(
            profile.ProfileId,
            profile.Slug,
            profile.Title,
            profile.Bio,
            profile.ViewsCount);
    }

    public async Task<ProfileResponse?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var profile = await repository.GetProfileBySlugAsync(
            slug.Trim().ToLowerInvariant(),
            cancellationToken);

        return profile is null
            ? null
            : new ProfileResponse(
                profile.ProfileId,
                profile.Slug,
                profile.Title,
                profile.Bio,
                profile.ViewsCount);
    }
}
