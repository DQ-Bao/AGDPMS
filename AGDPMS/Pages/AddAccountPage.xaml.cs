using AGDPMS.Shared.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AGDPMS.Services;

namespace AGDPMS.Pages;

public partial class AddAccountViewModel : ObservableObject
{
    private readonly IApiClient apiClient;

    private string phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => phoneNumber;
        set => SetProperty(ref phoneNumber, value);
    }

    private string fullName = string.Empty;
    public string FullName
    {
        get => fullName;
        set => SetProperty(ref fullName, value);
    }

    private RoleDto? selectedRole;
    public RoleDto? SelectedRole
    {
        get => selectedRole;
        set => SetProperty(ref selectedRole, value);
    }

    private IEnumerable<RoleDto> roles = Array.Empty<RoleDto>();
    public IEnumerable<RoleDto> Roles
    {
        get => roles;
        set => SetProperty(ref roles, value);
    }

    private string? message;
    public string? Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public AddAccountViewModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
        PhoneNumber = string.Empty;
        FullName = string.Empty;
        Roles = Array.Empty<RoleDto>();
        LoadRoles();
    }

    private async void LoadRoles()
    {
        try
        {
            var rolesResult = await apiClient.GetRolesAsync();
            if (rolesResult != null)
            {
                Roles = rolesResult;
            }
        }
        catch (Exception ex)
        {
            Message = $"Failed to load roles: {ex.Message}";
            OnPropertyChanged(nameof(HasMessage));
        }
    }

    [RelayCommand]
    private async Task CreateAccount()
    {
        Message = null;
        
        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            Message = "Phone number is required";
            OnPropertyChanged(nameof(HasMessage));
            return;
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            Message = "Full name is required";
            OnPropertyChanged(nameof(HasMessage));
            return;
        }

        if (SelectedRole == null)
        {
            Message = "Please select a role";
            OnPropertyChanged(nameof(HasMessage));
            return;
        }

        try
        {
            var result = await apiClient.AddAccountAsync(new AddAccountRequest
            {
                PhoneNumber = PhoneNumber,
                FullName = FullName,
                RoleId = SelectedRole.Id
            });

            if (result?.Success == true)
            {
                var window = Application.Current!.Windows.FirstOrDefault();
                if (window?.Page is ContentPage currentPage)
                {
                    await currentPage.DisplayAlert("Success", "Account created successfully", "OK");
                    await currentPage.Navigation.PopAsync();
                }
            }
            else
            {
                Message = result?.Message ?? "Account creation failed";
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
    private async Task Cancel()
    {
        var window = Application.Current!.Windows.FirstOrDefault();
        if (window?.Page is ContentPage currentPage)
        {
            await currentPage.Navigation.PopAsync();
        }
    }
}

public partial class AddAccountPage : ContentPage
{
    public AddAccountPage(AddAccountViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
