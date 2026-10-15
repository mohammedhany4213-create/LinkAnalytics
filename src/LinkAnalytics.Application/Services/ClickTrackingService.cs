using LinkAnalytics.Application.Interfaces;

namespace LinkAnalytics.Application.Services;

public sealed class ClickTrackingService(ILinkAnalyticsRepository repository) : IClickTrackingService
{
    public Task<string?> TrackAndGetDestinationAsync(
        int linkId,
        string countryCode,
        CancellationToken cancellationToken = default)
    {
        if (linkId <= 0)
            return Task.FromResult<string?>(null);

        var normalizedCountryCode = NormalizeCountryCode(countryCode);

        return repository.TrackClickAndGetDestinationAsync(
            linkId,
            normalizedCountryCode,
            cancellationToken);
    }

    private static string NormalizeCountryCode(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            return "XX";

        return countryCode.All(char.IsAsciiLetter)
            ? countryCode.ToUpperInvariant()
            : "XX";
    }
}
