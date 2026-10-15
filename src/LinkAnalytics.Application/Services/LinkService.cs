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
        if (request.ProfileId <= 0 || request.UserId <= 0 ||
            string.IsNullOrWhiteSpace(request.Title))
            return null;

        var title = request.Title.Trim();

        if (title.Length > 100 ||
            string.IsNullOrWhiteSpace(request.Url) ||
            request.Url.Length > 2048 ||
            !Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return null;

        if (!await repository.UserOwnsProfileAsync(request.ProfileId, request.UserId, cancellationToken))
            return null;

        var link = new Link
        {
            ProfileId = request.ProfileId,
            Title = title,
            Url = uri.ToString(),
            Order = await repository.GetNextLinkOrderAsync(request.ProfileId, cancellationToken)
        };

        await repository.AddLinkAsync(link, cancellationToken);
        return new LinkResponse(link.LinkId, link.Title, link.Url, link.Order);
    }

    public async Task<bool> UpdateAsync(
        int linkId,
        int userId,
        UpdateLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        if (linkId <= 0 || userId <= 0 || request is null ||
            string.IsNullOrWhiteSpace(request.Title) ||
            request.Title.Trim().Length > 100 ||
            string.IsNullOrWhiteSpace(request.Url) ||
            request.Url.Length > 2048 ||
            !Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return false;

        return await repository.UpdateLinkAsync(
            linkId, userId, request.Title.Trim(), uri.ToString(), cancellationToken);
    }

    public Task<bool> DeleteAsync(
        int linkId,
        int userId,
        CancellationToken cancellationToken = default) =>
        linkId <= 0 || userId <= 0
            ? Task.FromResult(false)
            : repository.DeleteLinkAsync(linkId, userId, cancellationToken);
}