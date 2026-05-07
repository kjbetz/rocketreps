# Rocket Reps

Rocket Reps is a .NET 10 Blazor web app for spaced-repetition study practice aimed at elementary and middle school students. The first product direction is a teacher-first classroom study tool where teachers create classrooms, assign learning decks, generate student logins, and students complete short review sessions.

The initial theme is space/rocket inspired for Riverview STEM Academy, whose mascot is the Rockets.

## Current Status

The app currently includes the foundation for the first vertical slice:

- ASP.NET Core Blazor app using Identity for authentication.
- PostgreSQL database wired through Aspire, with pgWeb available from the Aspire dashboard for local database inspection.
- EF Core domain schema for schools, classrooms, memberships, decks, cards, assignments, student card progress, and review logs.
- Startup seeding for `Admin`, `Teacher`, and `Student` roles plus `Riverview STEM Academy`.
- Ready-made global stock math decks for addition, subtraction, multiplication, and division facts, including full mixed decks and focused `0s`-`12s` practice decks where appropriate.
- Ready-made global stock `Spelling Lift-Off` deck with audio-prompt spelling practice.
- Ready-made global stock `California Facts` deck with multiple-choice social studies practice.
- Custom Rocket Reps landing page and responsive navigation using custom CSS instead of Bootstrap.
- Light/dark/system theme preference from a compact icon button in the top bar.
- `/decks` page that lists ready-made classroom decks.
- `/teacher/decks` teacher deck library for creating custom teacher-owned decks, publishing/unpublishing them, and opening deck workspaces.
- `/teacher/decks/{id}` deck workspace for editing custom deck details, adding/editing/deleting cards, and previewing cards before assigning them.
- `/pricing` page with teacher self-service Free, Pro, and Pro+ plan cards plus a school/district contact section.
- Teacher-focused registration that assigns the `Teacher` role.
- Role-aware post-login routing that sends teachers to `/teacher` and students to `/student` when no explicit return URL is present.
- `/teacher` classroom-first dashboard for classroom creation, classroom entry points, and deck library access.
- `/teacher/classrooms/{id}` classroom workspace for student login generation, ready-made deck assignment, active/inactive deck toggles, classroom progress snapshots, teacher attention signals, deck-level progress summaries, and roster progress review.
- `/student` dashboard that groups active classroom deck assignments into `Due Now`, `Ready For Launch`, and `All Caught Up` sections using due-card and new-card availability, with a per-deck mission details panel for status counts, attempts, correct totals, streaks, per-card status, and next due times.
- `/student/review/{assignmentId}` flow that records right/wrong reviews, shows lifetime correct counts after correct answers, keeps the answer input focused between typed cards, uses mobile-friendly numeric input for math facts, supports shuffled multiple-choice cards, supports audio-prompt spelling cards with browser speech synthesis and a local voice picker, schedules cards with FSRS.Core, and updates student card progress.
- Config-gated `/demo` launcher for open-house demos with seeded teacher, student, classroom, and deck assignment data.
- Postmark-backed Identity emails for teacher account confirmation and password resets.
- Plausible analytics rendered only in the Production environment.

## Product Direction

The current teacher-first flow is:

1. Teacher signs up with email and creates or joins a school/workspace.
2. Teacher creates a classroom from `/teacher`.
3. Teacher opens the classroom workspace.
4. Teacher generates student usernames and simple passwords inside that classroom.
5. Teacher creates optional custom decks from `/teacher/decks` and publishes them when ready.
6. Teacher assigns ready-made or published custom decks to the classroom.
7. Teacher marks classroom deck assignments active when students should work on them.
8. Student logs in with username and password, no student email required.
9. Student studies active assigned cards using a simple right/wrong interaction.
10. The app stores review history and schedules future reviews.
11. Students see lightweight progress, due-card status, streaks, and next practice windows.

Bulk deck import, card reordering, stock-deck copying, plan enforcement, checkout, and deeper deck-first/student-first teacher progress drilldowns are natural next steps. The current `/pricing` page is informational: teacher plan CTAs route to registration, and school/district pricing is a contact-us prompt. Admin functionality is intentionally deferred until school-level teacher management, billing, rostering, or reporting becomes necessary. The current workflow supports individual teacher usage first while keeping room for school/district administration later.

Teacher-created custom decks are owned by the teacher through `Deck.OwnerTeacherId`. Draft decks are editable but not assignable; published custom decks appear alongside ready-made decks in classroom assignment flows. Card authoring currently supports typed flashcards, math facts, multiple choice cards with `ChoicesJson`, and audio-prompt spelling cards. Card preview is read-only and is intended for teacher verification, not scheduling or student progress simulation.

Teacher-facing classroom progress should stay action-oriented instead of ranking students. The classroom workspace currently surfaces a quick snapshot, students who may need attention, deck assignment signals such as started/caught-up/due/not-started counts, recently tricky cards, and roster-level practice status. These summaries are computed from `StudentCardProgress` and `ReviewLog` so teachers can quickly decide who needs help starting, who needs practice time, and which cards may need reteaching.

For younger students, the first review model is binary: right or wrong. Internally this maps to scheduling-friendly ratings:

- Wrong: `Again`
- Right: `Good`

This keeps the experience simple while FSRS.Core handles the scheduling behind the scenes. Rocket Reps uses classroom-focused FSRS defaults: `0.9` desired retention, `1m` and `10m` learning steps, a `5m` relearning step, and a `365` day maximum interval.

Student review sessions select cards dynamically instead of walking the deck in sort order. Due cards appear first, oldest due first. If no cards are due, the app chooses a random new card. If there are no due or new cards, the student sees an all-done message. After every 20 reviewed cards, the app offers a break prompt when more work is available. Correct-answer feedback includes the student's lifetime correct count for that card as encouragement. Typed answer cards automatically focus the answer input when a card is ready, and math facts request a numeric keypad on mobile devices. Multiple-choice cards render large tap-friendly answer buttons and shuffle choices for each card attempt. Audio-prompt spelling cards hide the answer, require the student to tap `Hear word` on the first spelling card, and speak the target word through the browser's Web Speech API. After a student advances from one card to the next, audio-prompt spelling cards auto-play once and then focus the answer input after playback succeeds. Student deck details also avoid exposing audio-prompt answers by showing generic spelling-word labels instead of target words.

The spelling voice picker is intentionally client-side. `RocketReps.Web/wwwroot/studentReviewSpeech.js` reads voices from `speechSynthesis.getVoices()`, waits for the asynchronous `voiceschanged` event, and saves the selected `voiceURI` in local storage. Voice availability and pronunciation quality depend on the student's browser, operating system, installed voices, and sometimes network availability for cloud voices.

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

Demo mode is controlled by `Demo:Enabled`. When enabled, `/demo` shows one-click teacher and student demo launch buttons and startup seeds the demo accounts/classes if missing. The default `appsettings.json` currently enables this for temporary open-house use; set `Demo__Enabled=false` in the environment after the event to disable the launcher and demo login endpoints.

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
- Demo mode can be disabled in deployed env files with `Demo__Enabled=false` after temporary events.
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
Demo__Enabled=false
```

Mount `DataProtection__KeysDirectory` to persistent VPS storage for the web container. This preserves antiforgery and auth cookies across deploys. `Postmark__FromEmail` must be a verified sender signature or domain address in Postmark.

When a Podman network variable is set, `scripts/ci/apply-ef-bundle.sh` runs the EF bundle in a temporary Podman container attached to that network so it can resolve the database host.

## Test Workflow

To exercise the current vertical slice locally:

1. Register a teacher at `/Account/Register`.
2. Confirm the teacher account from the Postmark-delivered confirmation email.
3. Open `/teacher` and create a classroom.
4. Open the classroom workspace from the classroom card.
5. Generate a student login and save the displayed username/password.
6. Optionally open `/teacher/decks`, create a custom deck, add cards from the deck workspace, preview a card, and publish the deck.
7. Assign a ready-made or published custom deck to the classroom and leave it active, or activate it from the assignment card.
8. Log out and log in with the generated student username/password; the student should land on `/student`.
9. Start the active deck mission.

## Seed Data

Startup seeding creates missing baseline data in every environment:

- School: `Riverview STEM Academy`
- Roles: `Admin`, `Teacher`, `Student`
- Ready-made stock decks:
  - `Addition Launch Pad`
  - `Addition Launch Pad: 0s` through `Addition Launch Pad: 12s`
  - `Subtraction Orbit`
  - `Subtraction Orbit: 0s` through `Subtraction Orbit: 12s`
  - `Multiplication Mission`
  - `Multiplication Mission: 0s` through `Multiplication Mission: 12s`
  - `Division Docking`
  - `Division Docking: 1s` through `Division Docking: 12s`
  - `Spelling Lift-Off`
  - `California Facts`

The stock math decks generate facts programmatically instead of storing hundreds of seed rows in migrations. Division-focused decks start at `1s` because division by zero is not valid. The spelling deck seeds `CardType.AudioPrompt` cards for browser speech synthesis during student review. The California facts deck seeds `CardType.MultipleChoice` cards with answer choices in `ChoicesJson`; the student review flow shuffles choices per card attempt.

When `Demo:Enabled` is true, startup also seeds `demo.teacher`, `demo.student01` through `demo.student30`, two demo classrooms, and active assignments for math, spelling, and `California Facts` decks. The `/demo` page signs users into those accounts without showing passwords.

## Design Notes

- Do not reintroduce Bootstrap or a UI component library unless explicitly requested.
- Keep the visual language custom, modern, and kid-friendly with a space/rocket theme.
- Use Audiowide sparingly for the main brand/app-name treatment only; use Space Grotesk as the app-wide body/UI font.
- Keep the top bar compact and accessible; theme switching should remain an icon-button interaction that supports light, dark, and system preferences.
- Preserve privacy-minded defaults because the target users are elementary and middle school students.
- Prefer small vertical slices over broad platform features.
- Keep student auth on ASP.NET Core Identity, but expose a username/password experience created by teachers rather than student self-registration.
- Model deck availability per classroom through `DeckAssignment.IsActive`; do not use a global deck active flag.
