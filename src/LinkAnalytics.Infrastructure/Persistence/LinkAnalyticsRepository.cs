using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkAnalytics.Infrastructure.Persistence;

public sealed class LinkAnalyticsRepository(AppDbContext dbContext) : ILinkAnalyticsRepository
{
    public Task<bool> UserExistsAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(user => user.UserId == userId, cancellationToken);

    public Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default) =>
        dbContext.Profiles.AnyAsync(profile => profile.Slug == slug, cancellationToken);

    public Task<Profile?> GetProfileBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default) =>
        dbContext.Profiles
            .AsNoTracking()
            .Include(profile => profile.Links)
            .FirstOrDefaultAsync(profile => profile.Slug == slug, cancellationToken);

    public Task<bool> UserOwnsProfileAsync(
        int profileId,
        int userId,
        CancellationToken cancellationToken = default) =>
        dbContext.Profiles.AnyAsync(
            profile => profile.ProfileId == profileId && profile.UserId == userId,
            cancellationToken);

    public async Task<int> GetNextLinkOrderAsync(
        int profileId,
        CancellationToken cancellationToken = default)
    {
        var currentMaxOrder = await dbContext.Links
            .Where(link => link.ProfileId == profileId)
            .Select(link => (int?)link.Order)
            .MaxAsync(cancellationToken);

        return (currentMaxOrder ?? 0) + 1;
    }

    public async Task AddProfileAsync(
        Profile profile,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Profiles.AddAsync(profile, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLinkAsync(
        Link link,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Links.AddAsync(link, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
