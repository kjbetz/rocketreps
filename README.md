# Rocket Reps

Rocket Reps is a .NET 10 Blazor web app for spaced-repetition study practice aimed at elementary and middle school students. The first product direction is a classroom-based study tool where admins manage schools, teachers assign learning decks, and students complete short review sessions.

The initial theme is space/rocket inspired for Riverview STEM Academy, whose mascot is the Rockets.

## Current Status

The app currently includes the foundation for the first vertical slice:

- ASP.NET Core Blazor app using Identity for authentication.
- PostgreSQL database wired through Aspire.
- EF Core domain schema for schools, classrooms, memberships, decks, cards, assignments, student card progress, and review logs.
- Roles seeded for `Admin`, `Teacher`, and `Student`.
- Development data seeding for `Riverview STEM Academy`.
- Global stock math decks for addition, subtraction, multiplication, and division facts.
- Custom Rocket Reps landing page and responsive navigation using custom CSS instead of Bootstrap.
- `/decks` page that lists seeded stock decks.
- Protected Study Hub placeholder for the future student dashboard.

## Product Direction

The intended MVP flow is:

1. Admin creates or manages schools.
2. Teacher creates a classroom for a school.
3. Teacher creates student usernames or invites students with a classroom join code.
4. Teacher assigns stock or custom decks.
5. Student studies assigned cards using a simple right/wrong interaction.
6. The app stores review history and schedules future reviews.
7. Teacher sees lightweight progress and difficult-card signals.

For younger students, the first review model is binary: right or wrong. Internally this maps to scheduling-friendly ratings:

- Wrong: `Again`
- Right: `Good`

This keeps the experience simple while leaving room to integrate FSRS behind a scheduling abstraction later.

## Project Structure

- `apphost.cs`: file-based Aspire AppHost.
- `RocketReps.Web`: Blazor web application.
- `RocketReps.Web/Data`: EF Core Identity context, domain models, migrations, and seed data.
- `RocketReps.Web/Components`: Blazor routes, layout, account pages, and app UI.
- `RocketReps.ServiceDefaults`: Aspire service defaults for telemetry, resilience, discovery, and health endpoints.

## Local Development

Run the full Aspire app from the repository root:

```bash
aspire start --isolated
```

Build the web app and referenced projects:

```bash
dotnet build RocketReps.Web/RocketReps.Web.csproj
```

Add EF Core migrations from the repository root:

```bash
dotnet ef migrations add <Name> --project RocketReps.Web/RocketReps.Web.csproj --startup-project RocketReps.Web/RocketReps.Web.csproj --output-dir Data/Migrations
```

In development, pending migrations are applied automatically on startup. The local PostgreSQL resource is currently session-scoped, so local data may be recreated between Aspire sessions.

## Seed Data

Development startup seeding creates:

- School: `Riverview STEM Academy`
- Roles: `Admin`, `Teacher`, `Student`
- Stock decks:
  - `Addition Launch Pad`
  - `Subtraction Orbit`
  - `Multiplication Mission`
  - `Division Docking`

The stock math decks generate facts programmatically instead of storing hundreds of seed rows in migrations.

## Design Notes

- Do not reintroduce Bootstrap or a UI component library unless explicitly requested.
- Keep the visual language custom, modern, and kid-friendly with a space/rocket theme.
- Preserve privacy-minded defaults because the target users are elementary and middle school students.
- Prefer small vertical slices over broad platform features.
