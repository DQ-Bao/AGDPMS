using AGDPMS.Shared.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AGDPMS.Services;

namespace AGDPMS.Pages;

public partial class NeedChangePasswordViewModel : ObservableObject
{
    private readonly IApiClient apiClient;
    private readonly INavigation navigation;

    private string newPassword = string.Empty;
    public string NewPassword
    {
        get => newPassword;
        set => SetProperty(ref newPassword, value);
    }

    private string? message;
    public string? Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public int UserId { get; }

    public NeedChangePasswordViewModel(IApiClient apiClient, INavigation navigation, int userId)
    {
        this.apiClient = apiClient;
        this.navigation = navigation;
        UserId = userId;
        NewPassword = string.Empty;
    }

    [RelayCommand]
    private async Task ChangePassword()
    {
        Message = null;
        
        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            Message = "Password is required";
            OnPropertyChanged(nameof(HasMessage));
            return;
        }

        if (NewPassword.Length < 6)
        {
            Message = "Password must be at least 6 characters";
            OnPropertyChanged(nameof(HasMessage));
            return;
        }

        try
        {
            var result = await apiClient.ChangePasswordAsync(new ChangePasswordRequest
            {
                UserId = UserId,
                NewPassword = NewPassword
            });

            if (result?.Success == true)
            {
                var window = Application.Current!.Windows.FirstOrDefault();
                if (window?.Page is NavigationPage navPage)
                {
                    await navPage.DisplayAlert("Success", "Password changed successfully", "OK");
                    // Navigate to main page by replacing the navigation stack
                    var mainPage = new MainPage();
                    var newNavPage = new NavigationPage(mainPage) { Title = "AGDPMS" };
                    window.Page = newNavPage;
                }
            }
            else
            {
                Message = result?.Message ?? "Password change failed";
                OnPropertyChanged(nameof(HasMessage));
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            OnPropertyChanged(nameof(HasMessage));
        }
    }
}

public partial class NeedChangePasswordPage : ContentPage
{
    public NeedChangePasswordPage(NeedChangePasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
