using LinkAnalytics.Application.DTOs.Links;

namespace LinkAnalytics.Application.Interfaces;

public interface ILinkService
{
    Task<LinkResponse?> CreateAsync(CreateLinkRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int linkId, int userId, UpdateLinkRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int linkId, int userId, CancellationToken cancellationToken = default);
}