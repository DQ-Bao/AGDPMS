using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;

namespace AGDPMS
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var services = Current?.Handler?.MauiContext?.Services;
            var remembered = Preferences.Default.Get("remembered", false);
            Page rootPage;
            if (remembered)
            {
                rootPage = new MainPage();
            }
            else
            {
                var loginPage = services?.GetService<AGDPMS.Pages.LoginPage>();
                if (loginPage is null && services is not null)
                {
                    var vm = services.GetRequiredService<AGDPMS.Pages.LoginViewModel>();
                    loginPage = new AGDPMS.Pages.LoginPage(vm);
                }
                rootPage = loginPage as Page ?? new MainPage();
            }
            var nav = new NavigationPage(rootPage) { Title = "AGDPMS" };
            return new Window(nav) { Title = "AGDPMS" };
        }
    }
}
