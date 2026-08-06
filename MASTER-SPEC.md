You are a Principal Microsoft Solution Architect, Senior .NET MAUI Engineer, Senior Mobile UX Designer, and Enterprise Software Architect.

Your job is to build a production-quality cross-platform .NET MAUI application.

This is NOT a demo.
This is NOT a tutorial.
This is NOT sample code.

Everything must be production-ready.

====================================================
PROJECT
====================================================

Application Name

Attendance Scheduler

Platform

.NET 10
.NET MAUI

Target Platforms

Android
Android Tablet
iPhone
iPad

Architecture

Clean Architecture

Presentation

Application

Domain

Infrastructure

MVVM Toolkit

CommunityToolkit.Maui

Dependency Injection

HttpClientFactory

Repository Pattern

Service Layer

Shell Navigation

SQLite

SecureStorage

Preferences

Logging

====================================================
APPLICATION PURPOSE
====================================================

This application automatically performs attendance operations by calling REST APIs at scheduled times.

Morning Flow

Login
↓

Clock In
↓

Logout

Evening Flow

Login
↓

Clock Out
↓

Logout

These operations must happen automatically according to the schedule created by the user.

====================================================
RESPONSIVE UI
====================================================

The application must look identical across

Android Phone

Android Tablet

iPhone

iPad

Portrait

Landscape

Requirements

No broken layouts

No clipped controls

No overlapping controls

No horizontal scrolling

No duplicated layouts

One adaptive layout only

Use

Grid

VisualStateManager

AdaptiveTriggers

DeviceInfo.Idiom

Responsive spacing

Relative sizing

Never use fixed width controls.

====================================================
DESIGN STYLE
====================================================

Create a premium enterprise UI.

The UI should look similar in quality to Microsoft Outlook, Microsoft Teams, or Azure Mobile applications.

Design language

Rounded cards

Soft shadows

Modern typography

Large touch targets

Beautiful icons

Smooth animations

Material Icons

Modern color palette

Professional spacing

Consistent margins

Dark Mode

Light Mode

Skeleton loading

Lottie loading animation

Snackbars

Dialog animations

Empty states

Interactive cards

====================================================
PAGES
====================================================

1. Splash Screen

Animated logo

Loading indicator

Initialize services

Check authentication

Navigate automatically

----------------------------------------------------

2. Login

Company Logo

Welcome Text

Email

Password

Remember Me

Login Button

Forgot Password

Validation

Loading

Error handling

Dark mode

----------------------------------------------------

3. Dashboard

Beautiful summary cards

Scheduler Status

Today's Schedule

Clock In Time

Clock Out Time

Next Execution

Today's Result

Recent Execution

Quick Actions

Enable Scheduler

Pause Scheduler

Resume Scheduler

Stop Scheduler

Execution History

Recent Logs

Statistics placeholder

----------------------------------------------------

4. Schedule Management

Create Schedule

Edit Schedule

Delete Schedule

Enable Disable

Schedule Name

Daily

Weekly

Morning Time

Evening Time

Weekday Selection

Sunday

Monday

Tuesday

Wednesday

Thursday

Friday

Saturday

Validation

Beautiful Time Picker

----------------------------------------------------

5. Logs

Date

Time

Morning

Evening

Clock In Success

Clock Out Success

Duration

Error Message

Retry Count

Expandable cards

Search

Filter

----------------------------------------------------

6. Settings

API Base URL

Authentication

Timeout

Retry Count

Dark Mode

Notification Toggle

Battery Optimization Guide

Background Service Status

Version

====================================================
BACKGROUND EXECUTION
====================================================

Create a scheduler capable of executing attendance automatically.

Daily Scheduler

Weekly Scheduler

Morning execution

Evening execution

Execution should continue after device reboot where supported.

====================================================
ATTENDANCE FLOW
====================================================

Morning

Login

↓

Clock In

↓

Logout

Evening

Login

↓

Clock Out

↓

Logout

Never expose API calls directly inside scheduler.

Instead implement

AttendanceService

ClockInJob

ClockOutJob

SchedulerService

====================================================
API LAYER
====================================================

Create interfaces

ILoginService

IAttendanceService

ISchedulerService

ISettingsService

Create DTOs

LoginRequest

LoginResponse

ClockInRequest

ClockOutRequest

LogoutRequest

Response Models

Authentication Token

Refresh Token

Expiration

Store token securely.

Automatically attach Bearer Token to API requests.

Automatically logout.

====================================================
LOCAL STORAGE
====================================================

SQLite

Schedules

Execution Logs

Execution History

Preferences

SecureStorage

JWT Token

Credentials

====================================================
LOGGING
====================================================

Every execution must record

Date

Time

Success

Failure

HTTP Status

Duration

Request ID

Error Message

Retry Count

====================================================
PROJECT STRUCTURE
====================================================

Create folders

Pages

ViewModels

Models

Services

Repositories

Interfaces

DTOs

Database

Resources

Themes

Styles

Fonts

Converters

Helpers

Behaviors

Navigation

Configuration

====================================================
DELIVERABLE
====================================================

Generate the COMPLETE project.

Generate all folders.

Generate all files.

Generate every XAML page.

Generate every ViewModel.

Generate Dependency Injection.

Generate Themes.

Generate Styles.

Generate Navigation.

Generate SQLite layer.

Generate repository layer.

Generate service layer.

Generate mock data.

Generate everything required for compilation.

Do not leave TODO comments.

Do not leave placeholder methods.

Do not skip files.

Every class must compile successfully.

Follow Microsoft MAUI best practices.

Build the project incrementally, file by file, ensuring each part compiles before moving to the next.