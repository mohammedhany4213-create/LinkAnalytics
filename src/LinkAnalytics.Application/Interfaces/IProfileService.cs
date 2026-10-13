using LinkAnalytics.Application.DTOs.Profiles;

namespace LinkAnalytics.Application.Interfaces;

public interface IProfileService
{
    Task<ProfileResponse?> CreateAsync(
        CreateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfileResponse?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}
