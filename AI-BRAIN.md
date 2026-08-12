# Project Overview

Attendance Scheduler — a .NET MAUI (net10.0) mobile/desktop app that
automates a user's daily attendance workflow (Login → Clock In/Out →
Logout) on a user-defined schedule. Built with Clean Architecture + MVVM.
Targets Android, iOS, Mac Catalyst, Windows (MAUI does not support TV
platforms).

## Architecture

- **Models/** — domain entities persisted via sqlite-net-pcl (`Schedule`,
  `ExecutionLog`) plus supporting enums (`Weekday` flags, `ScheduleType`,
  `SchedulerState`, `ExecutionSession`, `ExecutionResult`).
- **DTOs/** — wire-shaped request/response contracts matching
  **API-CONTRACT.md** exactly. Decoupled from domain models.
- **Interfaces/** — `ILoginService`, `IAttendanceService`,
  `IAttendanceOrchestrator`, `IRetryPolicy`, `ISchedulerService`,
  `IBackgroundScheduler`, `INotificationService`, `ISettingsService`,
  `IScheduleRepository`, `IExecutionLogRepository`. ViewModels depend on
  interfaces only.
- **Services/** — `LoginService` and `AttendanceService` are now real
  `HttpClient` implementations (Phase 1's mocks were replaced). New in
  Phase 2: `AttendanceOrchestrator`, `RetryPolicyService`,
  `NotificationService`, and a rewritten `SchedulerService` that actually
  executes on a timer.
- **Repositories/**, **Database/** — unchanged from Phase 1.
- **Platforms/{Android,iOS,MacCatalyst,Windows}/BackgroundScheduler.cs** —
  one `IBackgroundScheduler` implementation per platform folder; MAUI's
  multi-targeting compiles only the matching file into each TFM, so
  `MauiProgram` registers `IBackgroundScheduler → BackgroundScheduler`
  once, unconditionally.
- **Platforms/Android/AttendanceWorker.cs** — WorkManager `Worker` that
  resolves the app's DI container and runs `ISchedulerService.TickAsync()`
  in the background.
- **ViewModels/**, **Pages/** — unchanged in shape from Phase 1; Dashboard
  now also subscribes to `ISchedulerService.ExecutionCompleted` for live
  updates after a background run completes while the app is foregrounded.

## Current Progress

Phase 2 (Automation) core is implemented against the contract documented
in **API-CONTRACT.md** — see that file for exact request/response shapes.
The user is implementing the backend to match this contract.

## Completed Features (Phase 2)

- Real `LoginService`: POST `/api/auth/login`, JWT + refresh token stored
  in `SecureStorage`, automatic refresh via `/api/auth/refresh` when the
  access token is within 1 minute of expiry
- Real `AttendanceService`: POST `/api/attendance/clock-in` /
  `/clock-out`, Bearer token attached automatically by `AuthTokenHandler`
- `AttendanceOrchestrator`: the single place that runs
  Login → Clock In/Out → Logout end to end, retries failures via
  `RetryPolicyService` (exponential backoff, capped 30s, count from
  `ISettingsService.RetryCount`), and writes one `ExecutionLog` row per
  attempt (HTTP status, duration, request ID, error, retry count)
- `SchedulerService` now actually executes: a 30s foreground timer checks
  all enabled schedules and runs any that are due (5-minute due window so
  a 30s cadence never misses a fire), de-duplicated per
  schedule/day/session so it can't double-run
- Background execution: Android via WorkManager (`OneTimeWorkRequest`,
  re-scheduled by `AttendanceWorker` after each run); iOS via
  `BGTaskScheduler` (registered in `AppDelegate`, submitted by
  `Platforms/iOS/BackgroundScheduler.cs`)
- Local notifications (`Plugin.LocalNotification`) on execution result,
  gated by the existing Settings toggle
- Credential-store opt-in tied to "Remember Me" (see Important Decisions)

## Pending / Follow-Up (not core automation gaps — deliberate, documented)

- **On-device testing.** This sandbox has no `dotnet` SDK, no
  Android/iOS/Windows SDKs, and no network access — nothing here has been
  compiled. See AI-HANDOVER.md for the exact first commands to run.
- **Windows / Mac Catalyst true background execution** (app fully closed,
  not just backgrounded) is a documented no-op for now —
  `Platforms/Windows/BackgroundScheduler.cs` and
  `Platforms/MacCatalyst/BackgroundScheduler.cs` explain why and what a
  real implementation would need (a Windows Background Task Runtime
  Component + manifest capability; a Mac Catalyst `BGTaskScheduler`
  registration). While the app process is alive on either platform, the
  foreground timer covers execution normally.
- Push notifications (server-initiated) — out of scope; only local
  device notifications are implemented.
- UI polish pass, automated testing, release/store packaging.

## Important Decisions

- **Contract-first backend.** `API-CONTRACT.md` is the source of truth
  for every request/response shape this app sends and expects. It was
  authored here (Phase 2) for the user to implement their backend
  against, rather than guessed at without a real API. If the real API
  ends up different, only `Services/LoginService.cs`,
  `Services/AttendanceService.cs`, and `Configuration/ApiEndpoints.cs`
  need to change.
- **Explicit credential store, gated behind "Remember Me".** Per
  MASTER-SPEC, the orchestrator logs out completely after *every*
  execution (Login → Clock → Logout, not Login → Clock → Clock). That
  means the access + refresh token are both cleared after each run, so
  the *next* scheduled run cannot succeed on a refresh token alone — it
  needs a fresh login. `ILoginService.SaveCredentialsAsync` /
  `LoginWithStoredCredentialsAsync` store the email/password in
  `SecureStorage` (OS keychain/keystore) only when the user checks
  "Remember Me" at login, and `AttendanceOrchestrator` falls back to them
  automatically when there's no valid session. Explicit logout from
  Settings clears both the live session *and* the stored credentials —
  it's a full, unambiguous sign-out. This is what makes true unattended
  twice-daily automation possible; it's opt-in and disclosed in the UI
  via the existing "Remember Me" toggle rather than silent.
- **Background execution is best-effort by OS design, not a bug.**
  Neither Android WorkManager nor iOS BGTaskScheduler guarantee exact
  timing — both batch wake-ups for battery reasons. The Settings page's
  "Battery Optimization Guide" exists because disabling battery
  optimization is the single biggest factor in reliable delivery on
  Android in particular.
- **Retry policy lives in one place.** `RetryPolicyService` is used by
  `AttendanceOrchestrator` only (not by `LoginService`'s refresh call,
  which has its own simple one-shot-then-fail logic) so retry behavior
  for the actual clock action is easy to reason about and test in
  isolation.
- **DI-resolved Shell pages** and **no custom icon font** decisions from
  Phase 1 are unchanged — see git history for that reasoning if needed.

## API Sequence (implemented)

Morning: `EnsureValidToken (refresh or stored-credential login) → ClockIn
→ Logout`. Evening: same with `ClockOut`. `SchedulerService.TickAsync()`
(foreground timer, or a platform background wake-up calling the same
method) triggers `AttendanceOrchestrator.ExecuteAsync`, which owns the
whole sequence and the resulting `ExecutionLog` row.

## Coding Style

Unchanged from Phase 1 — nullable reference types, CommunityToolkit.Mvvm
source generators, async all the way down, one ViewModel per page,
ViewModels depend on interfaces only.

## Folder Conventions

Unchanged from Phase 1, plus `Platforms/{Platform}/BackgroundScheduler.cs`
and `Platforms/Android/AttendanceWorker.cs` for platform-specific
background execution. `API-CONTRACT.md` and `BUILD.md` live at the repo
root alongside `MASTER-SPEC.md`.
