using System.Text.RegularExpressions;
using LinkAnalytics.Application.DTOs.Links;
using LinkAnalytics.Application.DTOs.Profiles;
using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Domain.Entities;

namespace LinkAnalytics.Application.Services;

public sealed class ProfileService(ILinkAnalyticsRepository repository) : IProfileService
{
    private static readonly Regex SlugPattern = new(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<ProfileResponse?> CreateAsync(CreateProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.Title))
            return null;

        var slug = request.Slug.Trim().ToLowerInvariant();
        var title = request.Title.Trim();
        var bio = request.Bio?.Trim() ?? string.Empty;

        if (!IsValidProfileData(slug, title, bio) ||
            !await repository.UserExistsAsync(request.UserId, cancellationToken) ||
            await repository.SlugExistsAsync(slug, cancellationToken))
            return null;

        var profile = new Profile { UserId = request.UserId, Slug = slug, Title = title, Bio = bio };
        await repository.AddProfileAsync(profile, cancellationToken);
        return MapToResponse(profile);
    }

    public async Task<ProfileResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var profile = await repository.GetProfileBySlugAsync(normalizedSlug, cancellationToken);
        if (profile is null)
            return null;

        await repository.IncrementProfileViewsAsync(normalizedSlug, cancellationToken);
        profile.ViewsCount++;
        return MapToResponse(profile);
    }

    public async Task<bool> UpdateAsync(
        int profileId, int userId, UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (profileId <= 0 || userId <= 0 || request is null ||
            string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.Title))
            return false;

        var slug = request.Slug.Trim().ToLowerInvariant();
        var title = request.Title.Trim();
        var bio = request.Bio?.Trim() ?? string.Empty;

        if (!IsValidProfileData(slug, title, bio) ||
            await repository.SlugExistsForAnotherProfileAsync(slug, profileId, cancellationToken))
            return false;

        return await repository.UpdateProfileAsync(profileId, userId, slug, title, bio, cancellationToken);
    }

    public Task<bool> DeleteAsync(int profileId, int userId, CancellationToken cancellationToken = default) =>
        profileId <= 0 || userId <= 0
            ? Task.FromResult(false)
            : repository.DeleteProfileAsync(profileId, userId, cancellationToken);

    private static bool IsValidProfileData(string slug, string title, string bio) =>
        slug.Length <= 50 && SlugPattern.IsMatch(slug) &&
        title.Length is > 0 and <= 100 && bio.Length <= 500;

    private static ProfileResponse MapToResponse(Profile profile) =>
        new(profile.ProfileId, profile.Slug, profile.Title, profile.Bio, profile.ViewsCount,
            profile.Links.OrderBy(link => link.Order)
                .Select(link => new LinkResponse(link.LinkId, link.Title, link.Url, link.Order)).ToArray());
}