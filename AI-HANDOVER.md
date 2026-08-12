# Session #6

## Completed

✔ Phase 2 — Automation, core implemented against a documented contract
  (`API-CONTRACT.md`) that the user will implement their backend against:

  - Real `LoginService` / `AttendanceService` (HttpClient, replacing
    Phase 1's mocks), JWT + refresh token handling
  - `AttendanceOrchestrator` running the full Login → Clock → Logout
    sequence with retry (`RetryPolicyService`, exponential backoff) and
    full `ExecutionLog` writes
  - `SchedulerService` rewritten to actually execute on a foreground
    timer, de-duplicated per schedule/day/session
  - Background execution: Android WorkManager (`AttendanceWorker` +
    `Platforms/Android/BackgroundScheduler.cs`), iOS `BGTaskScheduler`
    (`AppDelegate` registration + `Platforms/iOS/BackgroundScheduler.cs`)
  - Local notifications on execution result (`Plugin.LocalNotification`)
  - Explicit, opt-in credential store tied to "Remember Me" so unattended
    re-login works after the mandated post-run logout — see AI-BRAIN.md
    "Important Decisions" for exactly why this was necessary
  - `API-CONTRACT.md` (new) — the exact request/response shapes this
    build expects; build the backend to match
  - `BUILD.md` (new) — per-platform build/run/deploy instructions

✔ AI-BRAIN.md, ROADMAP.md updated for Phase 2.

## Pending (documented follow-ups, not silent gaps)

- **Nothing has been compiled.** This sandbox has no .NET SDK, no
  Android/iOS/Windows SDKs, and no network access. Run `dotnet restore &&
  dotnet build` first in a real environment — see BUILD.md §0. Static
  review was thorough but a compiler pass will likely surface a few
  binding-API naming details, especially in
  `Platforms/Android/BackgroundScheduler.cs` /
  `AttendanceWorker.cs` (AndroidX.Work bindings) and
  `Platforms/iOS/BackgroundScheduler.cs` (BackgroundTasks bindings) — main
  business logic (Services/, ViewModels/) is far lower-risk.
- Windows and Mac Catalyst background execution while the app is fully
  closed is a documented no-op for now (foreground timer covers it while
  running) — see those two `BackgroundScheduler.cs` files and
  AI-BRAIN.md for what a real implementation needs.
- No automated tests yet.
- No app store / release packaging (signing, icons for store listings,
  etc.) — BUILD.md covers local run/sideload only.
- TV platforms are not supported by .NET MAUI — not something to build
  toward on this stack.

## Known Issues

- Same environment caveats as Session #5 (no font file bundled — emoji
  glyphs used instead for icons; carried forward, not a regression).

## Next Step

1. Build the real backend against `API-CONTRACT.md`.
2. `dotnet restore && dotnet build` in a real .NET 10 + MAUI environment,
   fix whatever the compiler flags (see "Pending" above for where issues
   are most likely).
3. Point Settings → API Base URL at the backend, sign in with "Remember
   Me", create a schedule, and watch Logs for the first real executions.
4. Then: automated tests, UI polish, and release packaging per
   PHASE-02.md's remaining "Testing / Bug Fix / Polish / Release" items.

## Suggested Commit

(See git log — Phase 2 was committed incrementally with Conventional
Commits.)
