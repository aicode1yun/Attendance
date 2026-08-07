# Project Overview

Attendance Scheduler — a .NET MAUI (net10.0) mobile app that automates a
user's daily attendance workflow (Login → Clock In/Out → Logout) on a
user-defined schedule. Built with Clean Architecture + MVVM.

## Architecture

- **Models/** — domain entities persisted via sqlite-net-pcl (`Schedule`,
  `ExecutionLog`) plus supporting enums (`Weekday` flags, `ScheduleType`,
  `SchedulerState`, `ExecutionSession`, `ExecutionResult`).
- **DTOs/** — wire-shaped request/response contracts, decoupled from
  domain models, so a future real API integration only touches Services.
- **Interfaces/** — `ILoginService`, `IAttendanceService`,
  `ISchedulerService`, `ISettingsService`, `IScheduleRepository`,
  `IExecutionLogRepository`. All ViewModels depend on interfaces only.
- **Services/** — implementations. `LoginService` and `AttendanceService`
  are currently **mocked** (simulated latency + success), by design — see
  "Important decisions" below.
- **Repositories/** — thin async wrappers over `AppDatabase`
  (`SQLiteAsyncConnection`), one per entity.
- **Database/AppDatabase.cs** — owns the single SQLite connection and
  table creation; injected as a singleton.
- **ViewModels/** — CommunityToolkit.Mvvm `ObservableObject` +
  `[RelayCommand]`. One per page, plus a shared `BaseViewModel`
  (IsBusy/Title/Error).
- **Pages/** — XAML views, constructor-injected ViewModels, resolved by
  Shell via the DI container (`ContentTemplate="{DataTemplate pages:X}"`).
- **Navigation/Routes.cs** — centralised Shell route name constants.
- **Converters/**, **Behaviors/**, **Configuration/**, **Helpers/** —
  supporting infrastructure.

## Current Progress

Phase 1 (Foundation) is implemented: shell navigation, theming
(light/dark, enterprise indigo palette), Login, Dashboard, Schedule
list/edit, Logs, Settings, SQLite persistence, SecureStorage for the auth
token, and full DI wiring.

## Completed Features (Phase 1)

- Splash → auth check → Login or Dashboard routing
- Login (validation, remember-me, SecureStorage token, error state)
- Dashboard (scheduler status, next execution, today's schedule/result,
  quick actions, recent execution list)
- Schedule CRUD (name, daily/weekly, per-day toggles, morning/evening
  time pickers, enable switch) backed by SQLite
- Logs (search + session/result filter chips) backed by SQLite
- Settings (API base URL/timeout/retry config, dark mode, notifications
  toggle, background status display, battery-optimization guidance,
  logout)
- Responsive layout groundwork (VisualStateManager width breakpoint on
  Login)

## Pending Features (Phase 2 — Automation)

- Replace `LoginService`/`AttendanceService` mock bodies with real
  `HttpClient` calls against `ISettingsService.ApiBaseUrl`
- JWT refresh handling
- True OS-level background execution (Android WorkManager, iOS
  BGTaskScheduler) — `SchedulerService` currently only tracks in-process
  state and computes the next execution time; it does not yet survive
  app kill/reboot
- Retry policy with backoff, tied to `ExecutionLog.RetryCount`
- Push/local notifications on execution result
- Release polish

## Important Decisions

- **Mocked Phase 1 network layer.** `LoginService` and `AttendanceService`
  simulate their calls (delay + deterministic result) instead of calling
  a real endpoint. This lets the full UI/DB/DI stack be built and
  exercised now, and confines the "real unattended API automation" work
  to Phase 2, where it belongs per ROADMAP.md. Callers depend only on
  `ILoginService`/`IAttendanceService`, so swapping the implementation
  is a one-file change per service.
- **Scheduler is orchestration-only.** Per MASTER-SPEC, `SchedulerService`
  never calls the attendance API directly — it will call
  `IAttendanceService` for the middle step of Login → Clock/Logout in
  Phase 2. Phase 1 only implements the state machine
  (Running/Paused/Stopped) and next-execution calculation from stored
  schedules.
- **Ethics/scope note carried into Phase 2 planning:** the project's
  stated purpose is unattended automation of clock-in/out. Before wiring
  real background execution against a live employer API in Phase 2, the
  intended context (employer-sanctioned automation vs. spoofing
  presence) is worth a quick explicit confirmation with the user — noted
  here so it isn't silently skipped in a future session.
- **DI-resolved Shell pages.** All `ShellContent.ContentTemplate` values
  use `{DataTemplate pages:X}` and all pages are constructor-injected;
  this relies on MAUI Shell's documented DI resolution and avoids any
  service-locator code in code-behind.
- **No custom icon font.** Tab bar / header icons use Unicode emoji
  glyphs via `FontImageSource` (no bundled `.ttf` icon font) since this
  environment couldn't fetch one over the network. Swap in a proper icon
  font in Phase 2 polish if desired.

## API Sequence (planned, Phase 2)

Morning: `Login → ClockIn → Logout`. Evening: `Login → ClockOut →
Logout`. `SchedulerService` triggers the sequence; `ILoginService`
handles Login/Logout; `IAttendanceService` handles the single
ClockIn/ClockOut call in between. Bearer token is attached automatically
via `AuthTokenHandler` on the named `"AttendanceApi"` HttpClient.

## Coding Style

- Nullable reference types enabled, `ImplicitUsings` enabled.
- MVVM via CommunityToolkit.Mvvm source generators
  (`[ObservableProperty]`, `[RelayCommand]`) — no hand-written
  `INotifyPropertyChanged` boilerplate.
- Async all the way down for I/O (SQLite, SecureStorage, simulated
  network); commands are `async Task`, never `async void` except
  page-lifecycle event handlers.
- One ViewModel per page; ViewModels depend on interfaces, never
  concrete service/repository types.

## Folder Conventions

`Models/ DTOs/ Interfaces/ Services/ Repositories/ Database/ ViewModels/
Pages/ Converters/ Behaviors/ Navigation/ Configuration/ Helpers/` at the
project root, matching Clean Architecture layering. Route names live in
`Navigation/Routes.cs`; app-wide constants in `Configuration/AppConfig.cs`.
