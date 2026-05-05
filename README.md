# Rocket Reps

Rocket Reps is a .NET 10 Blazor web app for spaced-repetition study practice aimed at elementary and middle school students. The first product direction is a teacher-first classroom study tool where teachers create classrooms, assign learning decks, generate student logins, and students complete short review sessions.

The initial theme is space/rocket inspired for Riverview STEM Academy, whose mascot is the Rockets.

## Current Status

The app currently includes the foundation for the first vertical slice:

- ASP.NET Core Blazor app using Identity for authentication.
- PostgreSQL database wired through Aspire, with pgWeb available from the Aspire dashboard for local database inspection.
- EF Core domain schema for schools, classrooms, memberships, decks, cards, assignments, student card progress, and review logs.
- Startup seeding for `Admin`, `Teacher`, and `Student` roles plus `Riverview STEM Academy`.
- Global stock math decks for addition, subtraction, multiplication, and division facts.
- Custom Rocket Reps landing page and responsive navigation using custom CSS instead of Bootstrap.
- Light/dark/system theme preference from a compact icon button in the top bar.
- `/decks` page that lists seeded stock decks.
- Teacher-focused registration that assigns the `Teacher` role.
- Role-aware post-login routing that sends teachers to `/teacher` and students to `/student` when no explicit return URL is present.
- `/teacher` dashboard for classroom creation, student login generation, stock deck assignment, and active/inactive deck toggles.
- `/student` dashboard that shows active classroom deck assignments.
- `/student/review/{assignmentId}` flow that records right/wrong reviews, schedules cards with FSRS.Core, and updates student card progress.
- Postmark-backed Identity emails for teacher account confirmation and password resets.
- Plausible analytics rendered only in the Production environment.

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

This keeps the experience simple while FSRS.Core handles the scheduling behind the scenes. Rocket Reps uses classroom-focused FSRS defaults: `0.9` desired retention, `1m` and `10m` learning steps, a `5m` relearning step, and a `365` day maximum interval.

Student review sessions select cards dynamically instead of walking the deck in sort order. Due cards appear first, oldest due first. If no cards are due, the app chooses a random new card. If there are no due or new cards, the student sees an all-done message. After every 20 reviewed cards, the app offers a break prompt when more work is available.

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

Identity emails are sent through Postmark. For local email testing, configure the web project's user secrets or environment variables with a Postmark server token and a verified sender:

```bash
dotnet user-secrets set "Postmark:ServerToken" "..." --project RocketReps.Web/RocketReps.Web.csproj
dotnet user-secrets set "Postmark:FromEmail" "no-reply@example.com" --project RocketReps.Web/RocketReps.Web.csproj
dotnet user-secrets set "Postmark:MessageStream" "outbound" --project RocketReps.Web/RocketReps.Web.csproj
```

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
- App startup seeds identity roles, `Riverview STEM Academy`, and stock math decks after the deployment-applied schema is available.
- Plausible analytics is gated by `HostEnvironment.IsProduction()` and is not emitted in local development or staging.

Set these GitHub Actions variables for each environment:

- `STAGING_WEB_ENV_FILE` or `PRODUCTION_WEB_ENV_FILE`: path to the web app env file on the VPS.
- `STAGING_PODMAN_NETWORK` or `PRODUCTION_PODMAN_NETWORK`: Podman network where the database is reachable.

The web env file must include the Rocket Reps connection string and Postmark settings for account confirmation and password reset emails. It should also configure a mounted data protection keys directory:

```bash
ConnectionStrings__rocketreps=Host=...;Port=5432;Database=rocketreps;Username=...;Password=...
DataProtection__KeysDirectory=/var/rocketreps/data-protection-keys
Postmark__ServerToken=...
Postmark__FromEmail=no-reply@example.com
Postmark__MessageStream=outbound
```

Mount `DataProtection__KeysDirectory` to persistent VPS storage for the web container. This preserves antiforgery and auth cookies across deploys. `Postmark__FromEmail` must be a verified sender signature or domain address in Postmark.

When a Podman network variable is set, `scripts/ci/apply-ef-bundle.sh` runs the EF bundle in a temporary Podman container attached to that network so it can resolve the database host.

## Test Workflow

To exercise the current vertical slice locally:

1. Register a teacher at `/Account/Register`.
2. Confirm the teacher account from the Postmark-delivered confirmation email.
3. Open `/teacher` and create a classroom.
4. Generate a student login and save the displayed username/password.
5. Assign a stock deck to the classroom and leave it active, or activate it from the assignment card.
6. Log out and log in with the generated student username/password; the student should land on `/student`.
7. Start the active deck mission.

## Seed Data

Startup seeding creates missing baseline data in every environment:

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
- Keep the top bar compact and accessible; theme switching should remain an icon-button interaction that supports light, dark, and system preferences.
- Preserve privacy-minded defaults because the target users are elementary and middle school students.
- Prefer small vertical slices over broad platform features.
- Keep student auth on ASP.NET Core Identity, but expose a username/password experience created by teachers rather than student self-registration.
- Model deck availability per classroom through `DeckAssignment.IsActive`; do not use a global deck active flag.
