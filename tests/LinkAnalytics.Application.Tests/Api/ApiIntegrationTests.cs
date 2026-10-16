using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using LinkAnalytics.Application.DTOs.Links;
using LinkAnalytics.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace LinkAnalytics.Application.Tests.Api;

public sealed class ApiIntegrationTests
{
    private const string JwtKey = "integration-test-signing-key-must-be-at-least-32-bytes";
    
    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new LinkAnalyticsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/links/1/analytics");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        using var factory = new LinkAnalyticsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("user@example.com", "WrongPassword123"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Analytics_WithValidTokenAndMissingLink_ReturnsNotFound()
    {
        using var factory = new LinkAnalyticsApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken());

        var response = await client.GetAsync("/api/links/999/analytics");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidData_ReturnsBadRequest()
    {
        using var factory = new LinkAnalyticsApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken());

        var response = await client.PutAsJsonAsync(
            "/api/profiles/1",
            new { slug = "Invalid Slug!", title = "Profile", bio = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string CreateToken()
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "LinkAnalytics",
            audience: "LinkAnalytics.Client",
            claims: [new Claim(ClaimTypes.NameIdentifier, "1")],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class LinkAnalyticsApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = "Server=(localdb)\\mssqllocaldb;Database=LinkAnalyticsApiTests;Trusted_Connection=True;TrustServerCertificate=True",
                    ["Jwt:Key"] = JwtKey,
                    ["Jwt:Issuer"] = "LinkAnalytics",
                    ["Jwt:Audience"] = "LinkAnalytics.Client"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAuthService>();
                services.RemoveAll<IProfileService>();
                services.RemoveAll<ILinkService>();
                services.RemoveAll<IClickTrackingService>();
                services.RemoveAll<ILinkAnalyticsService>();

                var auth = new Mock<IAuthService>();
                auth.Setup(service => service.LoginAsync(
                        It.IsAny<LoginRequest>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync((AuthResponse?)null);

                var analytics = new Mock<ILinkAnalyticsService>();
                analytics.Setup(service => service.GetLinkAnalyticsAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync((LinkAnalyticsResponse?)null);

                services.AddSingleton(auth.Object);
                services.AddSingleton(Mock.Of<IProfileService>());
                services.AddSingleton(Mock.Of<ILinkService>());
                services.AddSingleton(Mock.Of<IClickTrackingService>());
                services.AddSingleton(analytics.Object);
            });
        }
    }
}
