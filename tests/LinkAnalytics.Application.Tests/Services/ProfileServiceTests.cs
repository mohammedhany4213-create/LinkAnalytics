using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Application.Services;
using LinkAnalytics.Domain.Entities;
using LinkAnalytics.Application.DTOs.Profiles;
using Moq;
using Xunit;

namespace LinkAnalytics.Application.Tests.Services;

public sealed class ProfileServiceTests
{
    [Theory]
    [InlineData("Invalid Slug")]
    [InlineData("-starts-with-hyphen")]
    [InlineData("ends-with-hyphen-")]
    [InlineData("contains_underscore")]
    public async Task CreateAsync_WhenSlugIsInvalid_DoesNotAccessRepository(string slug)
    {
        var repository = new Mock<ILinkAnalyticsRepository>(MockBehavior.Strict);
        var service = new ProfileService(repository.Object);

        var result = await service.CreateAsync(
            new CreateProfileRequest(1, slug, "My profile", null));

        Assert.Null(result);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_NormalizesInputAndCreatesProfile()
    {
        var repository = new Mock<ILinkAnalyticsRepository>();
        repository.Setup(x => x.UserExistsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repository.Setup(x => x.SlugExistsAsync("my-profile", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository.Setup(x => x.AddProfileAsync(
                It.IsAny<Profile>(),
                It.IsAny<CancellationToken>()))
            .Callback<Profile, CancellationToken>((profile, _) => profile.ProfileId = 12)
            .Returns(Task.CompletedTask);

        var service = new ProfileService(repository.Object);

        var result = await service.CreateAsync(
            new CreateProfileRequest(5, " My-Profile ", " My Page ", " About me "));

        Assert.NotNull(result);
        Assert.Equal(12, result.ProfileId);
        Assert.Equal("my-profile", result.Slug);
        Assert.Equal("My Page", result.Title);
        Assert.Equal("About me", result.Bio);
        repository.Verify(x => x.AddProfileAsync(
            It.Is<Profile>(p =>
                p.UserId == 5 &&
                p.Slug == "my-profile" &&
                p.Title == "My Page" &&
                p.Bio == "About me"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
