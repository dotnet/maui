using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Samples.View;

namespace Samples.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault },
        DataScheme = "xam")] // Shortcuts with a xam:// Uri (Automatically passed to Xamarin.Forms OnAppLinkRequestReceived()
    [IntentFilter(
        new[] { "battery" },
        Categories = new[] { Intent.CategoryDefault })] // Shortcuts without a Uri
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Xamarin.Essentials.Platform.Init(this, bundle);
            Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.Forms.FormsMaterial.Init(this, bundle);

            Xamarin.Essentials.Platform.ActivityStateChanged += Platform_ActivityStateChanged;

            LoadApplication(new App());
        }

        protected override void OnResume()
        {
            base.OnResume();

            Xamarin.Essentials.Platform.OnResume();

            // Handle shortcuts without a Uri here

            if (Intent?.Action == "battery")
            {
                Xamarin.Forms.Application.Current.MainPage.Navigation.PopToRootAsync();
                Xamarin.Forms.Application.Current.MainPage.Navigation.PushAsync(new BatteryPage());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Xamarin.Essentials.Platform.ActivityStateChanged -= Platform_ActivityStateChanged;
        }

        void Platform_ActivityStateChanged(object sender, Xamarin.Essentials.ActivityStateChangedEventArgs e) =>
            Toast.MakeText(this, e.State.ToString(), ToastLength.Short).Show();

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "xamarinessentials")]
    public class WebAuthenticationCallbackActivity : Xamarin.Essentials.WebAuthenticatorCallbackActivity
    {
    }
}
