# AGENTS.md

- This is a .NET 10 ASP.NET Core Blazor application orchestrated by a file-based Aspire AppHost (`apphost.cs`).
- Use `aspire start --isolated` from the repository root to run the AppHost locally.
- The file-based AppHost starts `RocketReps.Web` with the `https` launch profile so Aspire creates both HTTP and HTTPS endpoints and sets `ASPNETCORE_HTTPS_PORT` for local redirects.
- Use `dotnet build RocketReps.Web/RocketReps.Web.csproj` to build the web app and referenced ServiceDefaults project.
- The local Aspire topology includes PostgreSQL (`postgres`) with the `rocketrepsdb` database resource wired into `RocketReps.Web`.
- The web app uses `RocketReps.ServiceDefaults` for Aspire telemetry, service discovery, resilience, and health endpoints.
- `RocketReps.Web` uses Entity Framework Core Identity with PostgreSQL via `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`; do not reintroduce SQLite unless explicitly requested.
- Rocket Reps is a spaced-repetition classroom study app for elementary and middle school students, currently themed around Riverview STEM Academy Rockets.
- The current domain model includes schools, classrooms, classroom memberships, decks, cards, deck assignments, student card progress, and review logs.
- Development startup seeding lives in `RocketReps.Web/Data/ApplicationDataSeeder.cs` and creates `Riverview STEM Academy`, `Admin`/`Teacher`/`Student` roles, and stock math decks.
- Keep seeded stock decks programmatic unless there is a strong reason to move large content sets into migrations.
- The app intentionally uses custom CSS and should not use Bootstrap or a UI component library unless explicitly requested.
- The student review UX should stay age-appropriate. The first scheduling model is binary right/wrong, mapped internally to `Again`/`Good` ratings.
- Be privacy-conscious when adding student features; prefer teacher-created usernames and minimal student personal data.
- EF Core migrations live in `RocketReps.Web/Data/Migrations`. Add new schema migrations with `dotnet ef migrations add <Name> --project RocketReps.Web/RocketReps.Web.csproj --startup-project RocketReps.Web/RocketReps.Web.csproj --output-dir Data/Migrations`.
- In Development, `RocketReps.Web` applies pending EF Core migrations on startup so Aspire's session-scoped PostgreSQL database is initialized automatically.
- The PostgreSQL Aspire container is currently session-scoped and does not use a data volume; local data can be recreated between Aspire sessions.
- After changing compiled .NET code while Aspire is running, prefer `aspire resource web rebuild` over restarting the whole AppHost unless `apphost.cs` changed.
