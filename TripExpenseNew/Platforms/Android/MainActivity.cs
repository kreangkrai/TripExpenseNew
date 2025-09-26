using Android.App;
using Android.Content.PM;
using Android.OS;
using static Android.Content.Res.Resources;

namespace TripExpenseNew
{
    [Activity(Theme ="@style/Maui.SplashTheme" ,MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
