using LinkAnalytics.Domain.Entities;

namespace LinkAnalytics.Application.Interfaces;

public interface ILinkAnalyticsRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsForAnotherProfileAsync(string slug, int profileId, CancellationToken cancellationToken = default);
    Task<Profile?> GetProfileBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> UserOwnsProfileAsync(int profileId, int userId, CancellationToken cancellationToken = default);
    Task<Link?> GetLinkAnalyticsForUserAsync(int linkId, int userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(int profileId, int userId, string slug, string title, string bio, CancellationToken cancellationToken = default);
    Task<bool> DeleteProfileAsync(int profileId, int userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateLinkAsync(int linkId, int userId, string title, string url, CancellationToken cancellationToken = default);
    Task<bool> DeleteLinkAsync(int linkId, int userId, CancellationToken cancellationToken = default);
    Task<int> GetNextLinkOrderAsync(int profileId, CancellationToken cancellationToken = default);
    Task<bool> ReorderLinksAsync(int profileId, int userId, IReadOnlyList<int> linkIds, CancellationToken cancellationToken = default);
    Task AddProfileAsync(Profile profile, CancellationToken cancellationToken = default);
    Task AddLinkAsync(Link link, CancellationToken cancellationToken = default);
    Task IncrementProfileViewsAsync(string slug, CancellationToken cancellationToken = default);
    Task<string?> TrackClickAndGetDestinationAsync(int linkId, string countryCode, CancellationToken cancellationToken = default);
}