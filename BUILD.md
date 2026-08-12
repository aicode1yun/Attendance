# Build & Deploy Guide

This source tree was written and reviewed by hand in a sandbox with **no
.NET SDK and no network access** — it has not been compiled. This is the
first thing to do in a real environment, before anything platform-specific.

## 0. First build (any platform)

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download) and the MAUI
workload.

```bash
dotnet workload install maui
cd Attendance
dotnet restore
dotnet build
```

Fix anything the compiler flags — this is hand-written code that has had a
careful static review, not a compiler pass. Then build/run per platform
below.

---

## Android — phones & tablets

**Requires:** Android SDK (installed automatically by the `maui` workload,
or via Android Studio), a device with USB debugging enabled or an emulator.

```bash
dotnet build -f net10.0-android -t:Run
```

Or in Visual Studio / VS Code with the MAUI extension: select the
`net10.0-android` target and your device/emulator, then Run.

**To install a release APK on a real device:**
```bash
dotnet publish -f net10.0-android -c Release
# outputs an .apk / .aab under Attendance/bin/Release/net10.0-android/publish/
adb install bin/Release/net10.0-android/publish/com.companyname.attendance-Signed.apk
```
You'll need to sign the release build with your own keystore for
distribution outside `adb install` (Play Store requires this) — see
[Microsoft's Android signing docs](https://learn.microsoft.com/dotnet/maui/android/deployment/overview).

**After installing:** open Settings → "Battery Optimization Guide" and
disable battery optimization for the app. This matters a lot for
WorkManager's background wake-up reliability.

---

## iOS — iPhone & iPad

**Requires:** a Mac with Xcode, an Apple Developer account, a
provisioning profile / signing certificate.

```bash
dotnet build -f net10.0-ios -t:Run -p:_DeviceName=":v2:udid=<your-device-udid>"
```

Easier in practice: open `Attendance.sln` in Visual Studio (Mac or
Windows-with-paired-Mac) or Rider, select your device, and Run — Xcode
signing UI will prompt for your team/certificate the first time.

**Background execution note:** `BGTaskScheduler` wake-ups are
opportunistic — iOS decides when to actually run them based on usage
patterns and battery. This is expected iOS behavior, not a bug in this
app; Apple's own docs describe the same batching behavior for all apps.

---

## Mac Catalyst — Mac

```bash
dotnet build -f net10.0-maccatalyst -t:Run
```

Runs as a native Mac app. Code signing is required for distribution
outside your own Mac (same Apple Developer account as iOS).

---

## Windows — PC

**Requires:** Windows 10/11 with the Windows App SDK (installed by the
`maui` workload).

```bash
dotnet build -f net10.0-windows10.0.19041.0 -t:Run
```

Or open in Visual Studio on Windows, select the Windows target, and Run/
Deploy (F5). For a distributable package:
```bash
dotnet publish -f net10.0-windows10.0.19041.0 -c Release
```

---

## TVs

.NET MAUI does not support tvOS or Android TV as build targets. There is
no path to a TV build from this codebase without a substantially
different UI framework for that platform.

---

## Before your first real run against your API

1. Stand up your backend matching **API-CONTRACT.md** exactly (or update
   `Configuration/ApiEndpoints.cs` / the DTOs in `DTOs/` to match yours
   instead).
2. In the app, go to Settings and set **API Base URL** to your backend's
   URL, and adjust Timeout/Retry if needed.
3. Sign in once with "Remember Me" checked — this is required for
   unattended automation to keep working after the first Login → Clock →
   Logout cycle (see AI-BRAIN.md "Important Decisions" for why).
4. Create a schedule, enable it, and tap Enable on the Dashboard.
