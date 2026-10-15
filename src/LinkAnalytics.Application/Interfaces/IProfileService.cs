using LinkAnalytics.Application.DTOs.Profiles;

namespace LinkAnalytics.Application.Interfaces;

public interface IProfileService
{
    Task<ProfileResponse?> CreateAsync(CreateProfileRequest request, CancellationToken cancellationToken = default);
    Task<ProfileResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int profileId, int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int profileId, int userId, CancellationToken cancellationToken = default);
}