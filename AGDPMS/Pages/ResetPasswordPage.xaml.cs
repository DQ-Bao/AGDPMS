using AGDPMS.Shared.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AGDPMS.Services;

namespace AGDPMS.Pages;

public partial class ResetPasswordViewModel : ObservableObject
{
    private readonly IApiClient apiClient;

    private string newPassword = string.Empty;
    public string NewPassword
    {
        get => newPassword;
        set => SetProperty(ref newPassword, value);
    }

    private string confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => confirmPassword;
        set => SetProperty(ref confirmPassword, value);
    }

    private string? message;
    public string? Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public string ResetToken { get; }

    public ResetPasswordViewModel(IApiClient apiClient, string resetToken)
    {
        this.apiClient = apiClient;
        ResetToken = resetToken;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
    }

    [RelayCommand]
    private async Task Submit()
    {
        Message = null;
        
        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            Message = "Password is required";
            OnPropertyChanged(nameof(HasMessage));
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            Message = "Passwords do not match";
            OnPropertyChanged(nameof(HasMessage));
            return;
        }

        try
        {
            var result = await apiClient.ResetPasswordAsync(new ResetPasswordWithTokenRequest
            {
                Token = ResetToken,
                NewPassword = NewPassword,
                ConfirmPassword = ConfirmPassword
            });

            if (result?.Success == true)
            {
                var window = Application.Current!.Windows.FirstOrDefault();
                if (window?.Page is NavigationPage navPage)
                {
                    await navPage.DisplayAlert("Success", "Password reset successfully", "OK");
                    await navPage.PopToRootAsync();
                }
            }
            else
            {
                Message = result?.Message ?? "Password reset failed";
                OnPropertyChanged(nameof(HasMessage));
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            OnPropertyChanged(nameof(HasMessage));
        }
    }

    [RelayCommand]
    private async Task Back()
    {
        var window = Application.Current!.Windows.FirstOrDefault();
        if (window?.Page is NavigationPage navPage)
        {
            await navPage.PopAsync();
        }
    }
}

public partial class ResetPasswordPage : ContentPage
{
    public ResetPasswordPage(ResetPasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
