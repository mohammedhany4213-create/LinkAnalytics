using LinkAnalytics.Application.DTOs.Links;
using LinkAnalytics.Application.Interfaces;

namespace LinkAnalytics.Application.Services;

public sealed class LinkAnalyticsService(ILinkAnalyticsRepository repository) : ILinkAnalyticsService
{
    public async Task<LinkAnalyticsResponse?> GetLinkAnalyticsAsync(
        int linkId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (linkId <= 0 || userId <= 0)
            return null;

        var link = await repository.GetLinkAnalyticsForUserAsync(
            linkId,
            userId,
            cancellationToken);

        if (link is null)
            return null;

        var countryClicks = link.CountryClicks
            .OrderByDescending(click => click.Clicks)
            .Select(click => new CountryClickResponse(click.Country, click.Clicks))
            .ToArray();

        var totalClicks = countryClicks.Sum(click => click.Clicks);
        var clickThroughRate = link.Profile.ViewsCount == 0
            ? 0
            : Math.Round(totalClicks * 100d / link.Profile.ViewsCount, 2);

        return new LinkAnalyticsResponse(
            link.LinkId,
            link.Title,
            link.Url,
            totalClicks,
            countryClicks,
            clickThroughRate);
    }
}