using System.Text.RegularExpressions;
using Attendance.DTOs;
using Attendance.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Attendance.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly ILoginService _loginService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    private bool isPasswordHidden = true;

    public LoginViewModel(ILoginService loginService, ISettingsService settingsService)
    {
        _loginService = loginService;
        _settingsService = settingsService;
        Title = "Sign In";
        RememberMe = _settingsService.RememberMe;
    }

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        ClearError();

        if (!IsValidEmail(Email))
        {
            SetError("Please enter a valid email address.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 4)
        {
            SetError("Password must be at least 4 characters.");
            return;
        }

        try
        {
            IsBusy = true;

            var response = await _loginService.LoginAsync(new LoginRequest
            {
                Email = Email.Trim(),
                Password = Password
            });

            if (!response.Success)
            {
                SetError(response.ErrorMessage ?? "Unable to sign in. Please try again.");
                return;
            }

            _settingsService.RememberMe = RememberMe;

            if (RememberMe)
                await _loginService.SaveCredentialsAsync(Email.Trim(), Password);
            else
                await _loginService.ClearStoredCredentialsAsync();

            await Shell.Current.GoToAsync("//DashboardPage");
        }
        catch (Exception ex)
        {
            SetError($"Something went wrong: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task ForgotPasswordAsync()
    {
        if (Application.Current?.Windows.Count > 0)
        {
            var page = Application.Current.Windows[0].Page;
            if (page is not null)
            {
                await page.DisplayAlert(
                    "Forgot Password",
                    "Please contact your administrator to reset your password.",
                    "OK");
            }
        }
    }

    private static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) &&
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}
