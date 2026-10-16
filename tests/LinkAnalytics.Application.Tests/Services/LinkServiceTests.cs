using LinkAnalytics.Application.DTOs.Links;
using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Application.Services;
using LinkAnalytics.Domain.Entities;
using Moq;
using Xunit;

namespace LinkAnalytics.Application.Tests.Services;

public sealed class LinkServiceTests
{
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///C:/secret.txt")]
    [InlineData("not-a-url")]
    public async Task CreateAsync_WhenUrlIsNotHttpOrHttps_ReturnsNull(string url)
    {
        var repository = new Mock<ILinkAnalyticsRepository>(MockBehavior.Strict);
        var service = new LinkService(repository.Object);

        var result = await service.CreateAsync(
            new CreateLinkRequest(1, 2, "Website", url));

        Assert.Null(result);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_WhenUserDoesNotOwnProfile_ReturnsNull()
    {
        var repository = new Mock<ILinkAnalyticsRepository>();
        repository.Setup(x => x.UserOwnsProfileAsync(4, 8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new LinkService(repository.Object);

        var result = await service.CreateAsync(
            new CreateLinkRequest(4, 8, "Website", "https://example.com"));

        Assert.Null(result);
        repository.Verify(x => x.AddLinkAsync(
            It.IsAny<Link>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_CreatesLinkWithNextOrder()
    {
        var repository = new Mock<ILinkAnalyticsRepository>();
        repository.Setup(x => x.UserOwnsProfileAsync(4, 8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repository.Setup(x => x.GetNextLinkOrderAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
        repository.Setup(x => x.AddLinkAsync(It.IsAny<Link>(), It.IsAny<CancellationToken>()))
            .Callback<Link, CancellationToken>((link, _) => link.LinkId = 15)
            .Returns(Task.CompletedTask);

        var service = new LinkService(repository.Object);

        var result = await service.CreateAsync(
            new CreateLinkRequest(4, 8, " My Website ", " https://example.com/path "));

        Assert.NotNull(result);
        Assert.Equal(15, result.LinkId);
        Assert.Equal("My Website", result.Title);
        Assert.Equal("https://example.com/path", result.Url);
        Assert.Equal(3, result.Order);
        repository.Verify(x => x.AddLinkAsync(
            It.Is<Link>(link =>
                link.ProfileId == 4 &&
                link.Title == "My Website" &&
                link.Order == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUrlUsesUnsupportedScheme_ReturnsFalseWithoutCallingRepository()
    {
        var repository = new Mock<ILinkAnalyticsRepository>(MockBehavior.Strict);
        var service = new LinkService(repository.Object);

        var result = await service.UpdateAsync(
            10, 5, new UpdateLinkRequest("Website", "javascript:alert(1)"));

        Assert.False(result);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsValid_DelegatesToRepository()
    {
        var repository = new Mock<ILinkAnalyticsRepository>();
        repository.Setup(x => x.UpdateLinkAsync(
                10, 5, "Website", "https://example.com/path", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new LinkService(repository.Object);

        var result = await service.UpdateAsync(
            10, 5, new UpdateLinkRequest(" Website ", "https://example.com/path"));

        Assert.True(result);
        repository.Verify(x => x.UpdateLinkAsync(
            10, 5, "Website", "https://example.com/path", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenIdentifierIsInvalid_DoesNotCallRepository()
    {
        var repository = new Mock<ILinkAnalyticsRepository>(MockBehavior.Strict);
        var service = new LinkService(repository.Object);

        var result = await service.DeleteAsync(0, 5);

        Assert.False(result);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReorderAsync_WhenLinkIdsContainDuplicates_DoesNotCallRepository()
    {
        var repository = new Mock<ILinkAnalyticsRepository>(MockBehavior.Strict);
        var service = new LinkService(repository.Object);

        var result = await service.ReorderAsync(
            4, 8, new ReorderLinksRequest([2, 1, 2]));

        Assert.False(result);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReorderAsync_WhenRequestIsValid_DelegatesToRepository()
    {
        var repository = new Mock<ILinkAnalyticsRepository>();
        repository.Setup(x => x.ReorderLinksAsync(
                4, 8, It.Is<IReadOnlyList<int>>(ids => ids.SequenceEqual(new[] { 3, 1, 2 })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new LinkService(repository.Object);

        var result = await service.ReorderAsync(
            4, 8, new ReorderLinksRequest([3, 1, 2]));

        Assert.True(result);
    }
}
