# Community Sports Facilities Booking System

A web-based booking platform for a municipal Community Sports Council, built for
university coursework. Guests can browse a restricted facility catalogue and reviews and
submit inquiries; registered members can search facilities in full detail, book them
with automatic conflict detection, manage their booking history, and leave reviews.

## Technology Stack

- **Backend**: C# / ASP.NET Core MVC on .NET 10, Razor views
- **Database**: Microsoft SQL Server (SQL Server Express or LocalDB for local dev)
- **Data access**: Entity Framework Core (`Microsoft.EntityFrameworkCore.SqlServer`) +
  migrations, plus standalone SQL scripts under `database/`
- **Auth**: ASP.NET Core Identity (cookie-based)
- **Frontend**: Bootstrap 5 (CDN), Razor, minimal vanilla JS
- **Testing**: xUnit (`tests/CommunitySportsBookingSystem.Tests`)
- **Development environment**: VS Code + the `dotnet` CLI (no Visual Studio required)

## Prerequisites

- [.NET SDK 10.x](https://dotnet.microsoft.com/download) (`dotnet --version` should
  print `10.x`)
- Microsoft SQL Server reachable from your machine — either:
  - **SQL Server Express**, running as a Windows service, instance name `SQLEXPRESS`
    (the default this project's connection string targets), or
  - **SQL Server LocalDB** (`(localdb)\MSSQLLocalDB`) as a lighter-weight fallback
- The `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef` (or
  `dotnet tool update --global dotnet-ef` if you already have an older version)

## Configure the Connection String

The committed `src/CommunitySportsBookingSystem/appsettings.json` only has a
placeholder connection string. Create (or edit) an **untracked**
`src/CommunitySportsBookingSystem/appsettings.Development.json` with your real one:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=CommunitySportsBookingSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

If you're using LocalDB instead, use:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CommunitySportsBookingSystem;Trusted_Connection=True;TrustServerCertificate=True;"
```

This file is listed in `.gitignore` and must never be committed with real credentials.

## Restore, Build, Migrate, Run

All commands run from the repository root in a terminal (VS Code's integrated terminal
works fine — no Visual Studio menus needed):

```bash
# Restore NuGet packages
dotnet restore

# Build the whole solution
dotnet build

# Create/update the database schema (creates the database if it doesn't exist yet)
dotnet ef database update --project src/CommunitySportsBookingSystem

# Run the application
dotnet run --project src/CommunitySportsBookingSystem
```

The app prints the URL it's listening on (e.g. `https://localhost:5001`). On first run
it seeds five sports, five facilities, and a demo member account (see below).

## Run the Automated Tests

```bash
dotnet test tests/CommunitySportsBookingSystem.Tests
```

## Sample / Test Account

The app seeds a demo member account on startup (see
`src/CommunitySportsBookingSystem/Data/SeedData.cs`) so you can log in immediately
without registering first:

- **Email**: `demo.member@example.com`
- **Password**: `Demo@Pass1`

This account has a past Completed booking (so you can try submitting a review right
away) and an upcoming Confirmed booking (so you can try cancelling one).

## Making Schema Changes

If you change an entity in `Models/` or `Data/ApplicationDbContext.cs`, create and apply
a new migration:

```bash
dotnet ef migrations add <DescriptiveName> --project src/CommunitySportsBookingSystem
dotnet ef database update --project src/CommunitySportsBookingSystem
```

## Standalone SQL Server Scripts

`database/` contains hand-written SQL scripts, independent of the EF Core migrations,
as separate coursework evidence of the database design (see
`docs/database-implementation.md` for why both exist). Run them in order against your
SQL Server instance, e.g.:

```bash
sqlcmd -S "localhost\SQLEXPRESS" -i database/01-create-database.sql -C
sqlcmd -S "localhost\SQLEXPRESS" -i database/02-create-tables.sql -C
sqlcmd -S "localhost\SQLEXPRESS" -i database/03-seed-data.sql -C
sqlcmd -S "localhost\SQLEXPRESS" -i database/04-select-queries.sql -C
sqlcmd -S "localhost\SQLEXPRESS" -i database/05-testing-queries.sql -C
```

(The VS Code "SQL Server (mssql)" extension can run these interactively instead, if you
prefer a GUI over `sqlcmd`.)

## Project Structure

```
src/CommunitySportsBookingSystem/   ASP.NET Core MVC application
  Controllers/                      HomeController, AccountController, FacilitiesController,
                                     BookingsController, ReviewsController, InquiriesController
  Data/                             ApplicationDbContext
  Models/                           EF Core entities (ApplicationUser, Sport, Facility, Booking, ...)
  ViewModels/                       Form/view-specific models (never bind entities directly)
  Services/                         IBookingAvailabilityService (conflict detection)
  Views/                            Razor views, one folder per controller
  Migrations/                       EF Core migrations
tests/CommunitySportsBookingSystem.Tests/   xUnit tests for BookingAvailabilityService
database/                           Standalone SQL Server scripts (create/seed/select/testing)
docs/                                Coursework documentation (see below)
```

## Documentation

See `docs/` for the full coursework documentation set:

- `requirements.md` — functional/non-functional requirements summary
- `architecture.md` — MVC architecture and design decisions
- `erd.md` — Entity Relationship Diagram (Mermaid) and cardinalities
- `data-dictionary.md` — full column-level schema documentation
- `database-implementation.md` — SQL Server implementation notes
- `application-implementation.md` — feature-by-feature implementation walkthrough
- `testing.md` — full test matrix with genuinely executed results
- `screenshot-checklist.md` — manual screenshot capture list for the report
- `requirements-traceability.md` — requirement → DB/implementation/test/report mapping
- `coursework-report.md` — the assembled coursework report (with a Reflection
  placeholder for the student to complete personally)

Full specification, technical plan, and task breakdown live in
`specs/001-sports-facilities-booking/`.
