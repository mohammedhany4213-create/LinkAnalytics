namespace LinkAnalytics.Application.Interfaces;

public interface IClickTrackingService
{
    Task<string?> TrackAndGetDestinationAsync(
        int linkId,
        CancellationToken cancellationToken = default);
}
