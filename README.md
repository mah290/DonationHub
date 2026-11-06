# DonationHub

A donation platform API built with **ASP.NET Core (.NET 10)** using **Clean Architecture**.
Donors support campaigns, admins and campaign managers manage categories and campaigns.

## Table of Contents

- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Secrets and Configuration](#secrets-and-configuration)
- [API Documentation (Scalar)](#api-documentation-scalar)
- [Design Decisions](#design-decisions)

## Architecture

```
DonationHub.Domain          (no external dependencies)
        ↑
DonationHub.Application     (references Domain)
        ↑
DonationHub.Infrastructure  (references Application and Domain)
        ↑
DonationHub.Api             (composition root; references Application and Infrastructure)
```

| Project | Responsibility |
|---|---|
| **Domain** | Entities, value objects, enums, domain invariants |
| **Application** | Feature folders (vertical slices), abstractions, DTOs, services |
| **Infrastructure** | EF Core, Identity, JWT, repositories, seeding |
| **Api** | Controllers, authentication setup, error handling, OpenAPI/Scalar |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or a full instance)
- Visual Studio 2026 (or any editor with the .NET CLI)
- EF Core CLI tool (once):

  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Getting Started

### 1. Clone and restore

```bash
git clone https://github.com/mah290/DonationHub.git
cd DonationHub
dotnet restore
```

### 2. Configure the connection string

The default in `DonationHub.Api/appsettings.json` targets LocalDB:

```json
"Database": {
  "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=DonationHub;Trusted_Connection=True;TrustServerCertificate=True"
}
```

For SQL Authentication, put the full connection string (with the password) in user-secrets, not in the JSON file:

```bash
dotnet user-secrets set "Database:ConnectionString" "<your-connection-string>" --project DonationHub.Api
```

### 3. Set the secrets

See [Secrets and Configuration](#secrets-and-configuration). The application **will not start** until the required secrets are set.

### 4. Apply database migrations

```bash
dotnet ef database update --project DonationHub.Infrastructure --startup-project DonationHub.Api
```

Visual Studio equivalent (Package Manager Console, Default project = `DonationHub.Infrastructure`, Startup project = `DonationHub.Api`):

```powershell
Update-Database
```

To add a new migration:

```bash
dotnet ef migrations add <MigrationName> --project DonationHub.Infrastructure --startup-project DonationHub.Api --output-dir Persistence/Migrations
```

### 5. Run

```bash
dotnet run --project DonationHub.Api --launch-profile https
```

On startup (after migrations are applied) the seeder creates:

- Roles: `Admin`, `CampaignManager`, `Donor`
- The initial Admin user (from `SeedAdmin` options)
- In **Development only**: a Campaign Manager and a Donor test user (from `SeedDevUsers`) and five categories (Health, Education, Relief, Orphans, Environment)

## Secrets and Configuration

Settings are read only through strongly-typed option classes (`IOptions<T>`) and validated at startup. If a required value is missing or invalid, the application **fails fast** with an `OptionsValidationException`.

**Never commit real secrets.** `appsettings.json` contains placeholders only (empty secret values).

### Development: `dotnet user-secrets`

The Api project already has a `UserSecretsId`. Run these from the solution folder (replace the placeholders with your own values):

```bash
# JWT signing key (at least 32 characters)
dotnet user-secrets set "Jwt:SecretKey" "<random-key-at-least-32-chars>" --project DonationHub.Api

# Seeded Admin (email defaults to admin@donationhub.local in appsettings.json)
dotnet user-secrets set "SeedAdmin:Password" "<strong-password>" --project DonationHub.Api

# Dev-only test users (Development environment only)
dotnet user-secrets set "SeedDevUsers:CampaignManagerEmail" "manager@donationhub.local" --project DonationHub.Api
dotnet user-secrets set "SeedDevUsers:CampaignManagerPassword" "<password>" --project DonationHub.Api
dotnet user-secrets set "SeedDevUsers:DonorEmail" "donor@donationhub.local" --project DonationHub.Api
dotnet user-secrets set "SeedDevUsers:DonorPassword" "<password>" --project DonationHub.Api
```

Generate a random key (PowerShell):

```powershell
$bytes = New-Object byte[] 48; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes); [Convert]::ToBase64String($bytes)
```

Check what is set:

```bash
dotnet user-secrets list --project DonationHub.Api
```

> If the command cannot find `UserSecretsId`, use `--id <guid>` (the value from `DonationHub.Api.csproj`) instead of `--project`.

Password policy for all users: at least 8 characters, with an uppercase letter, a lowercase letter and a digit.

### Production: environment variables

Use `__` instead of `:`:

```
Jwt__SecretKey=...
SeedAdmin__Password=...
Database__ConnectionString=...
```

### Configuration reference

| Section | Keys | Notes |
|---|---|---|
| `Database` | `ConnectionString` | Required |
| `Jwt` | `SecretKey`, `Issuer`, `Audience`, `ExpiryMinutes` (15), `RefreshTokenExpiryDays` (7) | `SecretKey` comes from secrets |
| `SeedAdmin` | `Email`, `Password`, `FullName` | `Password` comes from secrets |
| `Donation` | `MinAmount` (1.00), `MaxAmount` (10000.00), `PendingTimeoutMinutes` (30), `DefaultCurrency` (USD) | `MaxAmount` must be >= `MinAmount` |
| `SeedDevUsers` | Manager and Donor email/password | Optional, Development only |

## API Documentation (Scalar)

In Development the API publishes an OpenAPI document and an interactive Scalar UI:

| URL | Purpose |
|---|---|
| `https://localhost:7149/scalar/v1` | Interactive documentation |
| `https://localhost:7149/openapi/v1.json` | Raw OpenAPI document |

(Ports come from `Properties/launchSettings.json`.) These endpoints are available **only in the Development environment**.

### Testing protected endpoints

1. Open `/scalar/v1`.
2. Open **Auth → `POST /api/v1/auth/login`** and send:

   ```json
   { "email": "admin@donationhub.local", "password": "<your SeedAdmin password>" }
   ```

3. Copy the `accessToken` from the response (without quotes).
4. Go to **Introduction → Authentication → Select Auth Type → Bearer** and paste the token (without the word `Bearer`).
5. Call a protected endpoint, for example `POST /api/v1/categories`.

Access tokens expire after 15 minutes (configurable). Use `POST /api/v1/auth/refresh` with the refresh token to get a new pair; each refresh token is single-use (rotation).

### Endpoints (base path `/api/v1`)

| Method | Path | Access |
|---|---|---|
| POST | `/auth/register` | Anonymous |
| POST | `/auth/login` | Anonymous |
| POST | `/auth/refresh` | Anonymous |
| POST | `/auth/logout` | Authenticated |
| GET | `/auth/me` | Authenticated |
| POST | `/admin/staff` | Admin |
| GET | `/categories`, `/categories/{id}` | Anonymous |
| POST, PUT, DELETE | `/categories`, `/categories/{id}` | Admin |

Errors are returned as ProblemDetails with a stable `code` (for example `Auth.InvalidCredentials`, `Category.HasCampaigns`).

## Design Decisions

### Writes: Generic Repository + Unit of Work. Reads: `DbContext` with `AsNoTracking` and DTO projection

(SRS §3.3 rule 5)

- **Writes** load entities with change tracking through `IGenericRepository<T>`, call behavior methods on the domain entity (which enforce the invariants), and persist everything once with `IUnitOfWork.SaveChangesAsync()`. The repository never calls `SaveChanges`, so several changes made by one handler are saved in a single transaction.
- **Reads** go through `IReadDbContext`, which exposes `AsNoTracking` queries. Handlers project directly to DTOs with `Select(...)`, so SQL selects only the needed columns and no change tracking overhead is paid. Entities are never returned from the API.
- **Why separate them?** Writes need tracked entities and domain rules. Reads need speed and shape. Forcing reads through a repository would add a layer without value and push the repository toward a "God repository".
- `IReadDbContext` and `IAsyncQueryExecutor` live in Application so it does not depend on EF Core.

### Other decisions

- **Dependency rule:** dependencies point inward only. Domain has no NuGet packages; `DateTime.UtcNow` is never used there. Time comes from `TimeProvider`, injected in services.
- **Rich domain model:** private setters, factory methods, and behavior methods; invariants throw `DomainException` with a stable code.
- **Soft delete:** `Campaign` implements `ISoftDeletable`. A `SaveChangesInterceptor` converts deletes to updates, and a global query filter hides deleted rows (`IgnoreQueryFilters()` to include them).
- **Identity:** `ApplicationUser : IdentityUser<Guid>` lives in Infrastructure, because the Domain cannot reference Identity packages. Other entities reference users by `Guid`.
- **Refresh tokens:** stored as SHA-256 hashes, rotated on every refresh (`RevokedAt` and `ReplacedByTokenId`), never stored in plain text.
- **Anti-enumeration:** login returns the same `Auth.InvalidCredentials` error for a wrong email or a wrong password.
- **Fail-fast configuration:** options use Data Annotations with `ValidateOnStart()`; secrets live in user-secrets or environment variables only.
- **Vertical slices:** Application code is organized by feature (`Features/Categories`, `Features/Auth`...) instead of technical folders.