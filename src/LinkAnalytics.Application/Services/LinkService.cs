using LinkAnalytics.Application.DTOs.Links;
using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Domain.Entities;

namespace LinkAnalytics.Application.Services;

public sealed class LinkService(ILinkAnalyticsRepository repository) : ILinkService
{
    public async Task<LinkResponse?> CreateAsync(
        CreateLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProfileId <= 0 ||
            request.UserId <= 0 ||
            string.IsNullOrWhiteSpace(request.Title) ||
            !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        if (!await repository.UserOwnsProfileAsync(
                request.ProfileId,
                request.UserId,
                cancellationToken))
        {
            return null;
        }

        var link = new Link
        {
            ProfileId = request.ProfileId,
            Title = request.Title.Trim(),
            Url = uri.ToString(),
            Order = await repository.GetNextLinkOrderAsync(
                request.ProfileId,
                cancellationToken)
        };

        await repository.AddLinkAsync(link, cancellationToken);

        return new LinkResponse(link.LinkId, link.Title, link.Url, link.Order);
    }
}
