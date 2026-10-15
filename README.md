# LinkAnalytics

A link-in-bio platform built with ASP.NET Core 10 Web API and Clean Architecture. Users can create public profiles, manage links, and track link engagement by country.

## Features

- JWT-based user registration and authentication.
- Multiple public profiles per user.
- Create, update, delete, and reorder profile links.
- Public profile pages with view tracking.
- Link redirects with country-based click aggregation.
- Per-link analytics: total clicks, clicks by country, and click-through rate.
- Entity Framework Core 10 with SQL Server.
- OpenAPI document in Development.

## Architecture

The solution is organized into four projects:

- **Domain** — entities and core business models.
- **Application** — DTOs, service contracts, and application services.
- **Infrastructure** — EF Core DbContext, configurations, migrations, and repository implementation.
- **API** — HTTP controllers, JWT authentication, dependency injection, and OpenAPI.

## Requirements

- .NET 10 SDK
- SQL Server (LocalDB or SQL Server Express)
- Entity Framework Core CLI

Install the EF Core CLI if it is not already installed:

```powershell
dotnet tool install --global dotnet-ef
```

## Configuration

The default SQL Server connection string is in `src/LinkAnalytics.Api/appsettings.json`.

Configure the JWT signing key using User Secrets (recommended for local development). From the repository root:

```powershell
dotnet user-secrets init --project src/LinkAnalytics.Api
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_A_RANDOM_SECRET_AT_LEAST_32_BYTES_LONG" --project src/LinkAnalytics.Api
```

The API will not start if the JWT key is missing or shorter than 32 bytes.

## Run locally

Restore and build:

```powershell
dotnet restore
dotnet build
```

Apply the existing EF Core migrations:

```powershell
dotnet ef database update --project src/LinkAnalytics.Infrastructure --startup-project src/LinkAnalytics.Api
```

Run the API:

```powershell
dotnet run --project src/LinkAnalytics.Api
```

When running in the Development environment, the OpenAPI document is available at:

```text
/openapi/v1.json
```

## API overview

### Authentication

| Method | Endpoint | Access |
| --- | --- | --- |
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |

### Profiles

| Method | Endpoint | Access |
| --- | --- | --- |
| POST | `/api/profiles` | Authenticated |
| GET | `/api/profiles/{slug}` | Public |
| PUT | `/api/profiles/{profileId}` | Owner |
| DELETE | `/api/profiles/{profileId}` | Owner |
| PUT | `/api/profiles/{profileId}/links/order` | Owner |

### Links

| Method | Endpoint | Access |
| --- | --- | --- |
| POST | `/api/links` | Authenticated |
| PUT | `/api/links/{linkId}` | Owner |
| DELETE | `/api/links/{linkId}` | Owner |
| GET | `/api/links/{linkId}/redirect` | Public |
| GET | `/api/links/{linkId}/analytics` | Owner |

Protected endpoints require:

```http
Authorization: Bearer <jwt-token>
```

## Tests

Run the automated application unit tests:

```powershell
dotnet test
```

The current test suite covers click-country normalization, link analytics calculations, and profile creation validation.

## Analytics notes

- Profile views are incremented when the public profile endpoint is requested.
- Redirect requests are aggregated by the two-character country code provided by the `CF-IPCountry` request header.
- Requests without a valid country code are recorded as `XX`.
- Click-through rate is calculated as total link clicks divided by profile views, multiplied by 100.

## License

No license has been added yet.
