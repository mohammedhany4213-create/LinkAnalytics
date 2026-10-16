using System.Text;
using LinkAnalytics.Application;
using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Api.Services;
using LinkAnalytics.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'Default' is not configured.");

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT signing key is not configured. Set Jwt__Key using a secret store or environment variable.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("JWT signing key must be at least 32 bytes long.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LinkAnalytics";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LinkAnalytics.Client";

if (builder.Environment.IsProduction())
{
    if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Issuer"]) ||
        string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Audience"]))
        throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be explicitly configured in Production.");

    if (connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains(".\\SQLEXPRESS", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("(local)", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Production must use a remotely hosted SQL Server connection string.");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<LinkAnalytics.Domain.Entities.User>, PasswordHasher<LinkAnalytics.Domain.Entities.User>>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsProduction())
    app.UseHsts();

app.MapGet("/", () => Results.Ok(new { message = "LinkAnalytics API is running" }));
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
