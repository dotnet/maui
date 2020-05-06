using System;
using System.Collections.Generic;
using System.Linq;
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

            AddAppActions();

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

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            if (uri?.Scheme != "xam")
            {
                base.OnAppLinkRequestReceived(uri);
                return;
            }

            MainPage.Navigation.PopToRootAsync();

            var page = uri.Segments.Last();

            switch (page)
            {
                case "appInfo":
                    MainPage.Navigation.PushAsync(new AppInfoPage());
                    break;
                case "deviceInfo":
                    MainPage.Navigation.PushAsync(new DeviceInfoPage());
                    break;
                default:
                    break;
            }
        }

        void AddAppActions()
        {
            AppActions.Actions = new List<AppAction>
            {
                new AppAction("app_info", "App Info", uri: new Uri("xam://samples/appInfo"), icon: "app_info_action_icon"),
                new AppAction("device_info", "Device Info", uri: new Uri("xam://samples/deviceInfo")), // Sample without an icon
                new AppAction("battery", "Battery", icon: "battery_action_icon") // Sample without a Uri - will require manually handling in AppDelegate and MainActivity
            };
        }
    }
}
