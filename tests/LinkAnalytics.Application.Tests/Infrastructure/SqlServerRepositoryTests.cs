using LinkAnalytics.Domain.Entities;
using LinkAnalytics.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LinkAnalytics.Application.Tests.Infrastructure;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LINKANALYTICS_TEST_SQLSERVER")))
            Skip = "Set LINKANALYTICS_TEST_SQLSERVER to a SQL Server connection string to run this test.";
    }
}

public sealed class SqlServerRepositoryTests
{
    [SqlServerFact]
    public async Task Migrations_CreateExpectedTablesAndIndexes()
    {
        await using var database = await TestDatabase.CreateAsync();

        var appliedMigrations = await database.Context.Database.GetAppliedMigrationsAsync();
        var model = database.Context.Model;

        Assert.NotEmpty(appliedMigrations);
        Assert.NotNull(model.FindEntityType(typeof(User)));
        Assert.NotNull(model.FindEntityType(typeof(Profile)));
        Assert.NotNull(model.FindEntityType(typeof(Link)));
        Assert.NotNull(model.FindEntityType(typeof(LinkCountryClick)));

        var clickIndexes = model.FindEntityType(typeof(LinkCountryClick))!.GetIndexes();
        Assert.Contains(clickIndexes, index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(LinkCountryClick.LinkId), nameof(LinkCountryClick.Country) }));
    }

    [SqlServerFact]
    public async Task TrackClick_ConcurrentRequests_AggregateWithoutLosingClicks()
    {
        await using var database = await TestDatabase.CreateAsync();
        var linkId = await database.SeedLinkAsync();
        const int concurrentClicks = 12;

        var operations = Enumerable.Range(0, concurrentClicks).Select(async _ =>
        {
            await using var context = database.CreateContext();
            var repository = new LinkAnalyticsRepository(context);
            var destination = await repository.TrackClickAndGetDestinationAsync(linkId, "EG");
            Assert.Equal("https://example.com", destination);
        });

        await Task.WhenAll(operations);

        await using var verificationContext = database.CreateContext();
        var click = await verificationContext.LinkCountryClicks
            .SingleAsync(item => item.LinkId == linkId && item.Country == "EG");

        Assert.Equal(concurrentClicks, click.Clicks);
    }

    [SqlServerFact]
    public async Task ReorderLinks_UpdatesOrderAndRejectsIncompleteLists()
    {
        await using var database = await TestDatabase.CreateAsync();
        var (profileId, linkIds) = await database.SeedProfileWithLinksAsync();

        await using (var context = database.CreateContext())
        {
            var repository = new LinkAnalyticsRepository(context);
            var reordered = await repository.ReorderLinksAsync(
                profileId, 1, new[] { linkIds[2], linkIds[0], linkIds[1] });

            Assert.True(reordered);
        }

        await using (var verificationContext = database.CreateContext())
        {
            var orderedIds = await verificationContext.Links
                .Where(link => link.ProfileId == profileId)
                .OrderBy(link => link.Order)
                .Select(link => link.LinkId)
                .ToArrayAsync();

            Assert.Equal(new[] { linkIds[2], linkIds[0], linkIds[1] }, orderedIds);
        }

        await using var invalidContext = database.CreateContext();
        var invalidRepository = new LinkAnalyticsRepository(invalidContext);
        var incomplete = await invalidRepository.ReorderLinksAsync(
            profileId, 1, new[] { linkIds[0], linkIds[1] });

        Assert.False(incomplete);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly DbContextOptions<AppDbContext> _options;

        private TestDatabase(DbContextOptions<AppDbContext> options)
        {
            _options = options;
            Context = CreateContext();
        }

        public AppDbContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var baseConnectionString = Environment.GetEnvironmentVariable("LINKANALYTICS_TEST_SQLSERVER")
                ?? throw new InvalidOperationException("SQL Server test connection string is missing.");

            var connection = new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = $"LinkAnalyticsTests_{Guid.NewGuid():N}"
            };

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connection.ConnectionString)
                .Options;

            var database = new TestDatabase(options);
            await database.Context.Database.MigrateAsync();
            return database;
        }

        public AppDbContext CreateContext() => new(_options);

        public async Task<int> SeedLinkAsync()
        {
            var (_, linkIds) = await SeedProfileWithLinksAsync();
            return linkIds[0];
        }

        public async Task<(int ProfileId, int[] LinkIds)> SeedProfileWithLinksAsync()
        {
            var user = new User
            {
                Name = "Integration Test User",
                Email = $"test-{Guid.NewGuid():N}@example.com",
                PasswordHash = "test-hash"
            };
            var profile = new Profile
            {
                User = user,
                Slug = $"test-{Guid.NewGuid():N}",
                Title = "Test Profile",
                Bio = string.Empty
            };
            profile.Links.Add(new Link { Title = "First", Url = "https://example.com", Order = 1 });
            profile.Links.Add(new Link { Title = "Second", Url = "https://example.org", Order = 2 });
            profile.Links.Add(new Link { Title = "Third", Url = "https://example.net", Order = 3 });

            Context.Users.Add(user);
            Context.Profiles.Add(profile);
            await Context.SaveChangesAsync();

            return (profile.ProfileId, profile.Links.OrderBy(link => link.Order).Select(link => link.LinkId).ToArray());
        }

        public async ValueTask DisposeAsync()
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.DisposeAsync();
        }
    }
}
