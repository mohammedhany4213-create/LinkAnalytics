using System.Data;
using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkAnalytics.Infrastructure.Persistence;

public sealed class LinkAnalyticsRepository(AppDbContext dbContext) : ILinkAnalyticsRepository
{
    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(user => user.UserId == userId, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
        dbContext.Profiles.AnyAsync(profile => profile.Slug == slug, cancellationToken);

    public Task<Profile?> GetProfileBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        dbContext.Profiles.AsNoTracking().Include(profile => profile.Links)
            .FirstOrDefaultAsync(profile => profile.Slug == slug, cancellationToken);

    public Task<bool> UserOwnsProfileAsync(int profileId, int userId, CancellationToken cancellationToken = default) =>
        dbContext.Profiles.AnyAsync(profile => profile.ProfileId == profileId && profile.UserId == userId, cancellationToken);

    public Task<Link?> GetLinkAnalyticsForUserAsync(
        int linkId,
        int userId,
        CancellationToken cancellationToken = default) =>
        dbContext.Links
            .AsNoTracking()
            .Include(link => link.Profile)
            .Include(link => link.CountryClicks)
            .FirstOrDefaultAsync(
                link => link.LinkId == linkId && link.Profile.UserId == userId,
                cancellationToken);

    public async Task<bool> UpdateLinkAsync(
        int linkId,
        int userId,
        string title,
        string url,
        CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Links
            .Where(link => link.LinkId == linkId && link.Profile.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(link => link.Title, title)
                    .SetProperty(link => link.Url, url),
                cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteLinkAsync(
        int linkId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Links
            .Where(link => link.LinkId == linkId && link.Profile.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        return affectedRows > 0;
    }

    public async Task<int> GetNextLinkOrderAsync(int profileId, CancellationToken cancellationToken = default)
    {
        var currentMaxOrder = await dbContext.Links.Where(link => link.ProfileId == profileId)
            .Select(link => (int?)link.Order).MaxAsync(cancellationToken);
        return (currentMaxOrder ?? 0) + 1;
    }

    public async Task AddProfileAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        await dbContext.Profiles.AddAsync(profile, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLinkAsync(Link link, CancellationToken cancellationToken = default)
    {
        await dbContext.Links.AddAsync(link, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task IncrementProfileViewsAsync(string slug, CancellationToken cancellationToken = default)
    {
        await dbContext.Profiles.Where(profile => profile.Slug == slug)
            .ExecuteUpdateAsync(setters => setters.SetProperty(profile => profile.ViewsCount, profile => profile.ViewsCount + 1), cancellationToken);
    }

    public async Task<string?> TrackClickAndGetDestinationAsync(int linkId, string countryCode, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var link = await dbContext.Links.AsNoTracking().Where(item => item.LinkId == linkId)
            .Select(item => new { item.Url }).SingleOrDefaultAsync(cancellationToken);
        if (link is null) return null;

        var updatedRows = await dbContext.LinkCountryClicks
            .Where(click => click.LinkId == linkId && click.Country == countryCode)
            .ExecuteUpdateAsync(setters => setters.SetProperty(click => click.Clicks, click => click.Clicks + 1), cancellationToken);

        if (updatedRows == 0)
        {
            await dbContext.LinkCountryClicks.AddAsync(new LinkCountryClick
            {
                LinkId = linkId,
                Country = countryCode,
                Clicks = 1
            }, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return link.Url;
    }
}