using AGDPMS.Shared.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AGDPMS.Services;
using System.Diagnostics;
using Microsoft.Maui.Storage;

namespace AGDPMS.Pages;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiClient apiClient;

    private string phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => phoneNumber;
        set => SetProperty(ref phoneNumber, value);
    }

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    private string? message;
    public string? Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    private bool rememberMe;
    public bool RememberMe
    {
        get => rememberMe;
        set => SetProperty(ref rememberMe, value);
    }

    public LoginViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
        PhoneNumber = string.Empty;
        Password = string.Empty;
    }

    [RelayCommand]
    private async Task Login()
    {
        Message = null;
        try
        {
            var result = await apiClient.LoginAsync(new LoginRequest
            {
                PhoneNumber = PhoneNumber,
                Password = Password
            });
            if (result is null || !result.Success)
            {
                Message = result?.Message ?? "Login failed";
                OnPropertyChanged(nameof(HasMessage));
                return;
            }

            // Check if user needs to change password
            if (result.NeedChangePassword)
            {
                var window = Application.Current!.Windows.FirstOrDefault();
                if (window?.Page is NavigationPage navPage)
                {
                    var vm = new NeedChangePasswordViewModel(apiClient, navPage.Navigation, result.UserId ?? 0);
                    await navPage.PushAsync(new NeedChangePasswordPage(vm));
                }
            }
            else
            {
                // Persist remember-me preference
                try
                {
                    Preferences.Default.Set("remembered", RememberMe);
                    if (RememberMe)
                    {
                        Preferences.Default.Set("rememberedUserId", result.UserId ?? 0);
                        Preferences.Default.Set("rememberedPhone", result.PhoneNumber ?? string.Empty);
                        Preferences.Default.Set("rememberedFullName", result.FullName ?? string.Empty);
                        Preferences.Default.Set("rememberedRole", result.RoleName ?? string.Empty);
                    }
                    else
                    {
                        Preferences.Default.Remove("rememberedUserId");
                        Preferences.Default.Remove("rememberedPhone");
                        Preferences.Default.Remove("rememberedFullName");
                        Preferences.Default.Remove("rememberedRole");
                    }
                }
                catch (Exception)
                {
                    // Non-fatal: if preferences fail, proceed with navigation
                }

                // Navigate to main page within the navigation stack
                var window = Application.Current!.Windows.FirstOrDefault();
                if (window?.Page is NavigationPage navPage)
                {
                    // Replace the entire navigation stack with MainPage
                    var mainPage = new MainPage();
                    var newNavPage = new NavigationPage(mainPage) { Title = "AGDPMS" };
                    window.Page = newNavPage;
                }
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            OnPropertyChanged(nameof(HasMessage));
        }
    }

    [RelayCommand]
    private async Task ForgotPassword()
    {
        try
        {
            Debug.WriteLine("ForgotPassword: Starting navigation");
            var window = Application.Current!.Windows.FirstOrDefault();
            Debug.WriteLine($"ForgotPassword: Window found: {window != null}");
            
            if (window?.Page is NavigationPage navPage)
            {
                Debug.WriteLine("ForgotPassword: NavigationPage found, getting services");
                var vm = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<ForgotPasswordViewModel>();
                Debug.WriteLine("ForgotPassword: ViewModel created successfully");
                await navPage.PushAsync(new ForgotPasswordPage(vm));
                Debug.WriteLine("ForgotPassword: Navigation completed");
            }
            else
            {
                Debug.WriteLine($"ForgotPassword: Page type: {window?.Page?.GetType().Name}");
                Message = "Unable to find navigation context";
                OnPropertyChanged(nameof(HasMessage));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ForgotPassword: Exception occurred: {ex.Message}");
            Debug.WriteLine($"ForgotPassword: Stack trace: {ex.StackTrace}");
            Message = $"Navigation error: {ex.Message}";
            OnPropertyChanged(nameof(HasMessage));
        }
    }
}

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

