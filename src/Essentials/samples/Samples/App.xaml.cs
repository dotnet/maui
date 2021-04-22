using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Essentials;
using Samples.Helpers;
using Samples.View;
using Device = Microsoft.Maui.Controls.Device;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Samples
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			VersionTracking.Track();

			MainPage = new NavigationPage(new HomePage());

			try
			{
				AppActions.OnAppAction += AppActions_OnAppAction;
			}
			catch (FeatureNotSupportedException ex)
			{
				Debug.WriteLine($"{nameof(AppActions)} Exception: {ex}");
			}
		}

		protected override async void OnStart()
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

			try
			{
				await AppActions.SetAsync(
					new AppAction("app_info", "App Info", icon: "app_info_action_icon"),
					new AppAction("battery_info", "Battery Info"));
			}
			catch (FeatureNotSupportedException ex)
			{
				Debug.WriteLine($"{nameof(AppActions)} Exception: {ex}");
			}
		}

		void AppActions_OnAppAction(object sender, AppActionEventArgs e)
		{
			// Don't handle events fired for old application instances
			// and cleanup the old instance's event handler

			if (Application.Current != this && Application.Current is App app)
			{
				AppActions.OnAppAction -= app.AppActions_OnAppAction;
				return;
			}

			Device.BeginInvokeOnMainThread(async () =>
			{
				var page = e.AppAction.Id switch
				{
					"battery_info" => new BatteryPage(),
					"app_info" => new AppInfoPage(),
					_ => default(Page)
				};

				if (page != null)
				{
					await Application.Current.MainPage.Navigation.PopToRootAsync();
					await Application.Current.MainPage.Navigation.PushAsync(page);
				}
			});
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
