# Attendance API Contract (Phase 2)

The app calls this contract via `ISettingsService.ApiBaseUrl` (set in
Settings). All endpoints are relative to that base URL. All request/response
bodies are JSON, `Content-Type: application/json`. All timestamps are ISO
8601 UTC (`"2026-08-12T09:00:00Z"`).

Implemented by: `Services/LoginService.cs`, `Services/AttendanceService.cs`.
Endpoint paths are centralized in `Configuration/ApiEndpoints.cs` — change
them there if your backend uses different paths.

---

## POST /api/auth/login

Request:
```json
{ "email": "user@company.com", "password": "secret" }
```

Response `200 OK`:
```json
{
  "success": true,
  "token": "eyJ...",
  "refreshToken": "rtk_...",
  "expiresAt": "2026-08-12T17:00:00Z"
}
```

Response `401 Unauthorized` (or any non-success status):
```json
{ "success": false, "errorMessage": "Invalid email or password." }
```

## POST /api/auth/refresh

Called automatically when the stored token is expired but a refresh token
exists.

Request:
```json
{ "refreshToken": "rtk_..." }
```

Response `200 OK`: same shape as login's success response.
Response `401 Unauthorized`: same shape as login's failure response — the
app treats this as "must log in again" and clears stored tokens.

## POST /api/auth/logout

Headers: `Authorization: Bearer {token}`

Request body: none.

Response `200 OK`:
```json
{ "success": true }
```

The app clears local tokens regardless of this call's outcome (logout is
best-effort server-side; it always succeeds locally).

## POST /api/attendance/clock-in

Headers: `Authorization: Bearer {token}` (attached automatically by
`AuthTokenHandler`)

Request:
```json
{ "timestamp": "2026-08-12T09:00:03Z" }
```

Response `200 OK`:
```json
{ "success": true, "requestId": "a1b2c3d4" }
```

Response error (any non-2xx):
```json
{ "success": false, "errorMessage": "reason" }
```

## POST /api/attendance/clock-out

Identical request/response shape to clock-in.

---

## Error handling / retry behaviour (client side)

- Any network exception, timeout, or non-2xx response is treated as a
  failure and retried per `ISettingsService.RetryCount` with exponential
  backoff (`Services/RetryPolicyService.cs`), unless the failure is a
  `401` from `/api/attendance/*` — in that case the app first attempts one
  token refresh via `/api/auth/refresh`, then retries the original call
  once before giving up.
- Every attempt (success or failure, including retries) is written to
  `ExecutionLog` (`HttpStatus`, `DurationMs`, `RequestId`, `ErrorMessage`,
  `RetryCount`) and visible on the Logs page.
- `ISettingsService.TimeoutSeconds` is applied as the `HttpClient` timeout
  for every call.
