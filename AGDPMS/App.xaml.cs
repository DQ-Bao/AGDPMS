using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;
using Microsoft.AspNetCore.Components.WebView.Maui;

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
            // Create BlazorWebView for Razor components
            var blazorWebView = new BlazorWebView
            {
                HostPage = "wwwroot/index.html"
            };
            
            // Set the root component to App.razor which handles routing
            blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(Components.App)
            });
            
            // Wrap BlazorWebView in a ContentPage
            var contentPage = new ContentPage
            {
                Content = blazorWebView,
                Title = "AGDPMS"
            };
            
            return new Window(contentPage) { Title = "AGDPMS" };
        }
    }
}
