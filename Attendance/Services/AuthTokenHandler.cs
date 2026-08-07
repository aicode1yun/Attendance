using System.Net.Http.Headers;

namespace Attendance.Services;

/// <summary>
/// Attaches the securely-stored bearer token to every outgoing request made
/// through the named "AttendanceApi" HttpClient, per MASTER-SPEC's
/// "Automatically attach Bearer Token to API requests" requirement.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await SecureStorage.Default.GetAsync("auth_token");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
