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

## Production deployment

The API can be built as a Linux container. Production configuration must be supplied through environment variables or your hosting provider's secret manager; never commit production credentials.

Required production settings:

| Environment variable | Purpose |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` |
| `ASPNETCORE_HTTP_PORTS` | Container HTTP port; defaults to `8080` |
| `ConnectionStrings__Default` | Remote SQL Server connection string |
| `Jwt__Key` | Random signing key of at least 32 bytes |
| `Jwt__Issuer` | Expected JWT issuer |
| `Jwt__Audience` | Expected JWT audience |

Production startup rejects missing JWT settings, short signing keys, and local SQL Server connection strings. Keep TLS termination at your hosting platform or reverse proxy and expose the API through HTTPS. The `/health` endpoint is available for container/platform health checks. OpenAPI remains disabled outside Development.

Build the container image from the repository root:

```powershell
docker build -t linkanalytics-api .
```

Run it by supplying secrets at runtime (replace the placeholders with values from your hosting provider):

```powershell
docker run --rm -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e ConnectionStrings__Default="$env:LINKANALYTICS_PRODUCTION_CONNECTION" `
  -e Jwt__Key="$env:LINKANALYTICS_JWT_KEY" `
  -e Jwt__Issuer="https://api.example.com" `
  -e Jwt__Audience="linkanalytics-client" `
  linkanalytics-api
```

Apply database migrations as a controlled deployment step before routing traffic to a new release. Do not automatically run migrations from every API instance at startup. Back up the production database before applying schema changes.

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

Run the complete automated test suite:

```powershell
dotnet test
```

The test suite includes:

- **Application unit tests** for click-country normalization, analytics calculations, link creation and updates, link ordering validation, and profile creation, update, and deletion.
- **API integration tests** using ASP.NET Core's test server for unauthenticated access, invalid login, JWT-protected endpoints, and API validation responses.
- **SQL Server repository integration tests** for applying migrations, verifying the relational model and unique indexes, concurrent click aggregation, and link reordering.

The SQL Server tests require a SQL Server instance with permission to create and drop databases. They are skipped when `LINKANALYTICS_TEST_SQLSERVER` is not set. For a local SQL Server Express instance, run:

```powershell
$env:LINKANALYTICS_TEST_SQLSERVER = "Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
dotnet test
```

Each repository test creates an isolated temporary database, applies the project's migrations, and deletes the database when the test completes. Do not point this variable at a production SQL Server instance.

## Analytics notes

- Profile views are incremented when the public profile endpoint is requested.
- Redirect requests are aggregated by the two-character country code provided by the `CF-IPCountry` request header.
- Requests without a valid country code are recorded as `XX`.
- Click-through rate is calculated as total link clicks divided by profile views, multiplied by 100.

## License

No license has been added yet.
