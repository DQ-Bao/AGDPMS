using AGDPMS.Shared.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AGDPMS.Services;

namespace AGDPMS.Pages;

public partial class VerifyOtpViewModel : ObservableObject
{
    private readonly IApiClient apiClient;

    private string otpCode = string.Empty;
    public string OtpCode
    {
        get => otpCode;
        set => SetProperty(ref otpCode, value);
    }

    private string? message;
    public string? Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public int UserId { get; }

    public VerifyOtpViewModel(IApiClient apiClient, int userId)
    {
        this.apiClient = apiClient;
        UserId = userId;
        OtpCode = string.Empty;
    }

    [RelayCommand]
    private async Task Verify()
    {
        Message = null;
        try
        {
            var result = await apiClient.VerifyOtpAsync(new VerifyOtpRequest
            {
                UserId = UserId,
                Otp = OtpCode
            });

            if (result?.Success == true && !string.IsNullOrEmpty(result.ResetToken))
            {
                // Navigate to reset password page
                var window = Application.Current!.Windows.FirstOrDefault();
                if (window?.Page is NavigationPage navPage)
                {
                    try
                    {
                        var resetVm = new ResetPasswordViewModel(apiClient, result.ResetToken);
                        var resetPage = new ResetPasswordPage(resetVm);
                        await navPage.PushAsync(resetPage);
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
                Message = result?.Message ?? "Verification failed";
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

public partial class VerifyOtpPage : ContentPage
{
    public VerifyOtpPage(VerifyOtpViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
