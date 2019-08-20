using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Samples.Helpers;
using Samples.View;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Device = Xamarin.Forms.Device;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Samples
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Enable currently experimental features

            VersionTracking.Track();

            MainPage = new NavigationPage(new HomePage());
        }

        protected override void OnStart()
        {
            if ((Device.RuntimePlatform == Device.Android && CommonConstants.AppCenterAndroid != "AC_ANDROID") ||
               (Device.RuntimePlatform == Device.iOS && CommonConstants.AppCenteriOS != "AC_IOS") ||
               (Device.RuntimePlatform == Device.UWP && CommonConstants.AppCenterUWP != "AC_UWP"))
            {
                AppCenter.Start(
                $"ios={CommonConstants.AppCenteriOS};" +
                $"android={CommonConstants.AppCenterAndroid};" +
                $"uwp={CommonConstants.AppCenterUWP}",
                typeof(Analytics),
                typeof(Crashes),
                typeof(Distribute));
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
