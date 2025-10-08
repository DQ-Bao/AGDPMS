using Android.App;
using Android.Content.PM;
using Android.OS;
using AGDPMS.Services;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;

namespace AGDPMS
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public override void OnBackPressed()
        {
            var history = Microsoft.Maui.IPlatformApplication.Current.Services.GetService(typeof(NavigationHistoryService)) as NavigationHistoryService;
            if (history != null && history.TryNavigateBack())
            {
                return;
            }

            base.OnBackPressed();
        }
    }
}
