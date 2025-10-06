using AGDPMS.Shared.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AGDPMS.Services;

namespace AGDPMS.Pages;

public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly IApiClient apiClient;

    private string phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => phoneNumber;
        set => SetProperty(ref phoneNumber, value);
    }

    private string? message;
    public string? Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public ForgotPasswordViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
        PhoneNumber = string.Empty;
    }

    [RelayCommand]
    private async Task SendSms()
    {
        Message = null;
        try
        {
            var result = await apiClient.ForgotPasswordAsync(new ForgotPasswordRequest
            {
                Phone = PhoneNumber
            });

            if (result?.Success == true && result.UserId.HasValue)
            {
                // Navigate to OTP verification page
                var window = Application.Current!.Windows.FirstOrDefault();
                if (window?.Page is NavigationPage navPage)
                {
                    try
                    {
                        var otpVm = new VerifyOtpViewModel(apiClient, result.UserId.Value);
                        var otpPage = new VerifyOtpPage(otpVm);
                        await navPage.PushAsync(otpPage);
                    }
                    catch (Exception navEx)
                    {
                        Message = $"Navigation error: {navEx.Message}";
                        OnPropertyChanged(nameof(HasMessage));
                    }
                }
                else
                {
                    Message = "Unable to find navigation context";
                    OnPropertyChanged(nameof(HasMessage));
                }
            }
            else
            {
                Message = result?.Message ?? "Failed to send SMS";
                OnPropertyChanged(nameof(HasMessage));
            }
        }
        catch (Exception ex)
        {
            Message = $"API Error: {ex.Message}";
            OnPropertyChanged(nameof(HasMessage));
        }
    }

    [RelayCommand]
    private async Task BackToLogin()
    {
        var window = Application.Current!.Windows.FirstOrDefault();
        if (window?.Page is NavigationPage navPage)
        {
            await navPage.PopAsync();
        }
    }
}

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage(ForgotPasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
