using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Application.Services;
using Moq;
using Xunit;

namespace LinkAnalytics.Application.Tests.Services;

public sealed class ClickTrackingServiceTests
{
    [Fact]
    public async Task TrackAndGetDestinationAsync_WhenLinkIdIsInvalid_ReturnsNullWithoutCallingRepository()
    {
        var repository = new Mock<ILinkAnalyticsRepository>(MockBehavior.Strict);
        var service = new ClickTrackingService(repository.Object);

        var result = await service.TrackAndGetDestinationAsync(0, "EG");

        Assert.Null(result);
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("eg", "EG")]
    [InlineData("US", "US")]
    [InlineData("", "XX")]
    [InlineData("EGY", "XX")]
    [InlineData("1A", "XX")]
    [InlineData(null, "XX")]
    public async Task TrackAndGetDestinationAsync_NormalizesCountryCode(
        string? inputCountryCode,
        string expectedCountryCode)
    {
        const int linkId = 10;
        const string destination = "https://example.com";

        var repository = new Mock<ILinkAnalyticsRepository>();
        repository
            .Setup(x => x.TrackClickAndGetDestinationAsync(
                linkId,
                expectedCountryCode,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);

        var service = new ClickTrackingService(repository.Object);

        var result = await service.TrackAndGetDestinationAsync(linkId, inputCountryCode!);

        Assert.Equal(destination, result);
        repository.Verify(
            x => x.TrackClickAndGetDestinationAsync(
                linkId,
                expectedCountryCode,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
