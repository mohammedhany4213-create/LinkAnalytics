using LinkAnalytics.Domain.Entities;

namespace LinkAnalytics.Application.Interfaces;

public interface ILinkAnalyticsRepository
{
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    Task<Profile?> GetProfileBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> UserOwnsProfileAsync(int profileId, int userId, CancellationToken cancellationToken = default);

    Task<int> GetNextLinkOrderAsync(int profileId, CancellationToken cancellationToken = default);

    Task AddProfileAsync(Profile profile, CancellationToken cancellationToken = default);

    Task AddLinkAsync(Link link, CancellationToken cancellationToken = default);

    Task IncrementProfileViewsAsync(string slug, CancellationToken cancellationToken = default);

    Task<string?> TrackClickAndGetDestinationAsync(
        int linkId,
        CancellationToken cancellationToken = default);
}
