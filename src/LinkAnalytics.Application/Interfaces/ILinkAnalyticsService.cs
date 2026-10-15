using LinkAnalytics.Application.DTOs.Links;

namespace LinkAnalytics.Application.Interfaces;

public interface ILinkAnalyticsService
{
    Task<LinkAnalyticsResponse?> GetLinkAnalyticsAsync(
        int linkId,
        int userId,
        CancellationToken cancellationToken = default);
}