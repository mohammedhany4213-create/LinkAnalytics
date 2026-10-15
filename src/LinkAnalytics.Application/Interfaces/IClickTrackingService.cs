namespace LinkAnalytics.Application.Interfaces;

public interface IClickTrackingService
{
    Task<string?> TrackAndGetDestinationAsync(
        int linkId,
        string countryCode,
        CancellationToken cancellationToken = default);
}
