using LinkAnalytics.Application.DTOs.Links;

namespace LinkAnalytics.Application.Interfaces;

public interface ILinkService
{
    Task<LinkResponse?> CreateAsync(
        CreateLinkRequest request,
        CancellationToken cancellationToken = default);
}
