using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace LinkAnalytics.Api.Services;

public sealed class AuthService(
    ILinkAnalyticsRepository repository,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (name.Length is < 2 or > 100 ||
            !IsValidEmail(email) ||
            request.Password.Length is < 8 or > 128)
            throw new ArgumentException("Name, email, or password is invalid.");

        if (await repository.GetUserByEmailAsync(email, cancellationToken) is not null)
            return null;

        var user = new User { Name = name, Email = email };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        await repository.AddUserAsync(user, cancellationToken);

        return CreateToken(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await repository.GetUserByEmailAsync(email, cancellationToken);

        if (user is null)
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        return result == PasswordVerificationResult.Failed ? null : CreateToken(user);
    }

    private AuthResponse CreateToken(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "LinkAnalytics";
        var audience = configuration["Jwt:Audience"] ?? "LinkAnalytics.Client";
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer, audience, claims, expires: expiresAt, signingCredentials: credentials);

        return new AuthResponse(
            user.UserId,
            user.Name,
            user.Email,
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }

    private static bool IsValidEmail(string email) =>
        email.Length <= 256 &&
        System.Text.RegularExpressions.Regex.IsMatch(
            email,
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
}