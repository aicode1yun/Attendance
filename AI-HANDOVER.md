# Session #5

## Completed

✔ Phase 1 — Foundation, fully implemented (see AI-BRAIN.md for detail):
  Clean Architecture folder structure, MVVM (CommunityToolkit.Mvvm),
  Shell navigation (Splash/Login outside tabs, Dashboard/Schedules/Logs/
  Settings as a TabBar), enterprise light/dark theme, Login, Dashboard,
  Schedule list + create/edit, Logs (search + filter), Settings, SQLite
  persistence (Schedule, ExecutionLog), SecureStorage-backed auth token,
  full DI wiring in MauiProgram.cs.

✔ AI-BRAIN.md updated with architecture, decisions, and Phase 2 plan.

✔ ROADMAP.md Phase 1 marked complete.

## Pending (Phase 2 — Automation, per ROADMAP.md)

- Real HTTP implementation of `ILoginService` / `IAttendanceService`
  (currently mocked — see AI-BRAIN.md "Important Decisions")
- JWT refresh handling
- True OS-level background scheduling (Android WorkManager / iOS
  BGTaskScheduler) — `SchedulerService` today only tracks in-process
  state, it does not survive app kill or device reboot
- Retry policy with backoff
- Execution notifications
- Testing pass, UI polish, release prep

## Known Issues / Environment Notes

- This development sandbox has **no `dotnet` SDK and no network access**
  (NuGet restore is blocked). All Phase 1 code was hand-written and
  manually reviewed for correctness but **has not been compiled**. First
  action in an environment with the .NET MAUI workload installed should
  be `dotnet restore && dotnet build -f net10.0-android` (or your target
  TFM) to catch anything a static review missed.
- No bundled icon font — tab bar / header icons use Unicode emoji via
  `FontImageSource` rather than a custom `.ttf`, since one couldn't be
  fetched here. Fine functionally; swap for a proper icon font as a
  Phase 2/polish item if desired.
- Android WorkManager not yet implemented (carried over from prior
  session note — still true, it's Phase 2 scope).

## Next Step

Begin Phase 2: start with `PHASE-02.md`'s "Real APIs" item — replace the
mock bodies of `LoginService.LoginAsync` and
`AttendanceService.ClockInAsync`/`ClockOutAsync` with real `HttpClient`
calls (the `AuthTokenHandler` and named `"AttendanceApi"` HttpClient are
already wired in `MauiProgram.cs`, so only the service bodies change).
Before wiring real background execution against a live employer
endpoint, confirm the intended deployment context with the user (see
AI-BRAIN.md decisions note).

## Suggested Commit

(See git log — Phase 1 was committed incrementally with Conventional
Commits; see `feat(...)` history.)
