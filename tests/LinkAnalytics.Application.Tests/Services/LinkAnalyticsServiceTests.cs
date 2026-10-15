using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Application.Services;
using LinkAnalytics.Domain.Entities;
using Moq;
using Xunit;

namespace LinkAnalytics.Application.Tests.Services;

public sealed class LinkAnalyticsServiceTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task GetLinkAnalyticsAsync_WhenIdentifiersAreInvalid_ReturnsNull(int linkId, int userId)
    {
        var repository = new Mock<ILinkAnalyticsRepository>(MockBehavior.Strict);
        var service = new LinkAnalyticsService(repository.Object);

        var result = await service.GetLinkAnalyticsAsync(linkId, userId);

        Assert.Null(result);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLinkAnalyticsAsync_WhenLinkDoesNotExist_ReturnsNull()
    {
        var repository = new Mock<ILinkAnalyticsRepository>();
        repository
            .Setup(x => x.GetLinkAnalyticsForUserAsync(7, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Link?)null);

        var service = new LinkAnalyticsService(repository.Object);

        var result = await service.GetLinkAnalyticsAsync(7, 3);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLinkAnalyticsAsync_ReturnsTotalClicksCountryBreakdownAndClickThroughRate()
    {
        var link = new Link
        {
            LinkId = 7,
            Title = "Portfolio",
            Url = "https://example.com",
            Profile = new Profile { ProfileId = 2, ViewsCount = 200 },
            CountryClicks =
            [
                new LinkCountryClick { Country = "US", Clicks = 30 },
                new LinkCountryClick { Country = "EG", Clicks = 70 }
            ]
        };

        var repository = new Mock<ILinkAnalyticsRepository>();
        repository
            .Setup(x => x.GetLinkAnalyticsForUserAsync(7, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(link);

        var service = new LinkAnalyticsService(repository.Object);

        var result = await service.GetLinkAnalyticsAsync(7, 3);

        Assert.NotNull(result);
        Assert.Equal(100, result.TotalClicks);
        Assert.Equal(50, result.ClickThroughRate);
        Assert.Equal("EG", result.CountryClicks.First().Country);
        Assert.Equal("Portfolio", result.Title);
    }

    [Fact]
    public async Task GetLinkAnalyticsAsync_WhenProfileHasNoViews_ReturnsZeroClickThroughRate()
    {
        var link = new Link
        {
            LinkId = 7,
            Profile = new Profile { ProfileId = 2, ViewsCount = 0 },
            CountryClicks = [new LinkCountryClick { Country = "EG", Clicks = 4 }]
        };

        var repository = new Mock<ILinkAnalyticsRepository>();
        repository
            .Setup(x => x.GetLinkAnalyticsForUserAsync(7, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(link);

        var service = new LinkAnalyticsService(repository.Object);

        var result = await service.GetLinkAnalyticsAsync(7, 3);

        Assert.NotNull(result);
        Assert.Equal(0, result.ClickThroughRate);
    }
}
