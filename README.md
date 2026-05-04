# Rocket Reps

Rocket Reps is a .NET 10 Blazor web app for spaced-repetition study practice aimed at elementary and middle school students. The first product direction is a teacher-first classroom study tool where teachers create classrooms, assign learning decks, generate student logins, and students complete short review sessions.

The initial theme is space/rocket inspired for Riverview STEM Academy, whose mascot is the Rockets.

## Current Status

The app currently includes the foundation for the first vertical slice:

- ASP.NET Core Blazor app using Identity for authentication.
- PostgreSQL database wired through Aspire, with pgWeb available from the Aspire dashboard for local database inspection.
- EF Core domain schema for schools, classrooms, memberships, decks, cards, assignments, student card progress, and review logs.
- Roles seeded for `Admin`, `Teacher`, and `Student`.
- Development data seeding for `Riverview STEM Academy`.
- Global stock math decks for addition, subtraction, multiplication, and division facts.
- Custom Rocket Reps landing page and responsive navigation using custom CSS instead of Bootstrap.
- `/decks` page that lists seeded stock decks.
- Teacher-focused registration that assigns the `Teacher` role.
- `/teacher` dashboard for classroom creation, student login generation, stock deck assignment, and active/inactive deck toggles.
- `/student` dashboard that shows active classroom deck assignments.
- `/student/review/{assignmentId}` flow that records right/wrong reviews and updates student card progress.

## Product Direction

The intended MVP flow is:

1. Teacher signs up with email and creates or joins a school/workspace.
2. Teacher creates a classroom.
3. Teacher generates student usernames and simple passwords.
4. Teacher assigns stock or custom decks to a classroom.
5. Teacher marks deck assignments active when students should work on them.
6. Student logs in with username and password, no student email required.
7. Student studies active assigned cards using a simple right/wrong interaction.
8. The app stores review history and schedules future reviews.
9. Teacher sees lightweight progress and difficult-card signals.

Admin functionality is intentionally deferred until school-level teacher management, billing, rostering, or reporting becomes necessary. The current workflow supports individual teacher usage first while keeping room for school/district administration later.

For younger students, the first review model is binary: right or wrong. Internally this maps to scheduling-friendly ratings:

- Wrong: `Again`
- Right: `Good`

This keeps the experience simple while leaving room to integrate FSRS behind a scheduling abstraction later.

## Project Structure

- `apphost.cs`: file-based Aspire AppHost.
- `RocketReps.Web`: Blazor web application.
- `RocketReps.Web/Data`: EF Core Identity context, domain models, migrations, and seed data.
- `RocketReps.Web/Components`: Blazor routes, layout, account pages, and app UI.
- `RocketReps.Web/Dockerfile`: production container image build for the web app.
- `RocketReps.ServiceDefaults`: Aspire service defaults for telemetry, resilience, discovery, and health endpoints.
- `scripts/ci/apply-ef-bundle.sh`: VPS-side EF migration bundle runner used by deploy workflows.
- `rocketreps.slnx`: .NET solution file for the repository.

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

The Aspire AppHost also starts pgWeb for the PostgreSQL resource. Open it from the Aspire dashboard when you need to inspect the local `rocketreps` database.

After changing compiled code while Aspire is running, rebuild the web resource instead of restarting the full AppHost when possible:

```bash
aspire resource web rebuild
```

## Deployment

GitHub Actions handles container builds, EF migration bundles, and VPS deploys:

- Staging runs on every push to `main`.
- Production runs only when a `v*` tag is pushed.
- Staging publishes `ghcr.io/kjbetz/rocketreps-web:staging` and `ghcr.io/kjbetz/rocketreps-web:sha-<commit>`.
- Production promotes the tagged commit's `:sha-<commit>` image to `:prod` and `:<version-tag>`.
- Deploy jobs run on the self-hosted runner labeled `self-hosted`, `linux`, and `rocketreps-vps`.
- EF migrations are applied from a generated linux-x64 bundle before `podman auto-update` runs.

Set these GitHub Actions variables for each environment:

- `STAGING_WEB_ENV_FILE` or `PRODUCTION_WEB_ENV_FILE`: path to the web app env file on the VPS.
- `STAGING_PODMAN_NETWORK` or `PRODUCTION_PODMAN_NETWORK`: Podman network where the database is reachable.

The web env file must include the Rocket Reps connection string and should configure a mounted data protection keys directory:

```bash
ConnectionStrings__rocketreps=Host=...;Port=5432;Database=rocketreps;Username=...;Password=...
DataProtection__KeysDirectory=/var/rocketreps/data-protection-keys
```

Mount `DataProtection__KeysDirectory` to persistent VPS storage for the web container. This preserves antiforgery and auth cookies across deploys.

When a Podman network variable is set, `scripts/ci/apply-ef-bundle.sh` runs the EF bundle in a temporary Podman container attached to that network so it can resolve the database host.

## Test Workflow

To exercise the current vertical slice locally:

1. Register a teacher at `/Account/Register`.
2. Confirm the teacher account using the development confirmation link.
3. Open `/teacher` and create a classroom.
4. Generate a student login and save the displayed username/password.
5. Assign a stock deck to the classroom and leave it active, or activate it from the assignment card.
6. Log out and log in with the generated student username/password.
7. Open `/student` and start the active deck mission.

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
- Keep student auth on ASP.NET Core Identity, but expose a username/password experience created by teachers rather than student self-registration.
- Model deck availability per classroom through `DeckAssignment.IsActive`; do not use a global deck active flag.
