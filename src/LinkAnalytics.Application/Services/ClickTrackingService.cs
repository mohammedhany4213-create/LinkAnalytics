using LinkAnalytics.Application.Interfaces;

namespace LinkAnalytics.Application.Services;

public sealed class ClickTrackingService(ILinkAnalyticsRepository repository) : IClickTrackingService
{
    public Task<string?> TrackAndGetDestinationAsync(
        int linkId,
        CancellationToken cancellationToken = default)
    {
        if (linkId <= 0)
            return Task.FromResult<string?>(null);

        return repository.TrackClickAndGetDestinationAsync(linkId, cancellationToken);
    }
}
