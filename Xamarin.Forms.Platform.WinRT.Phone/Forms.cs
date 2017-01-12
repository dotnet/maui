using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.WinRT;

namespace Xamarin.Forms
{
	public static class Forms
	{
		public static void Init (IActivatedEventArgs launchActivatedEventArgs)
		{
			if (s_isInitialized)
				return;
			
			var accent = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["SystemColorControlAccentBrush"];
			Color.Accent = Color.FromRgba (accent.Color.R, accent.Color.G, accent.Color.B, accent.Color.A);

			Log.Listeners.Add (new DelegateLogListener ((c, m) => Debug.WriteLine (LogFormat, c, m)));

			Windows.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add (GetPhoneResources());

			Device.PlatformServices = new WindowsPhonePlatformServices (Window.Current.Dispatcher);
			Device.Info = new WindowsDeviceInfo();
			Device.Idiom = TargetIdiom.Phone;
			
			Ticker.Default = new WindowsTicker();

			ExpressionSearch.Default = new WindowsExpressionSearch();

			Registrar.RegisterAll (new[] {
				typeof (ExportRendererAttribute),
				typeof (ExportCellAttribute),
				typeof (ExportImageSourceHandlerAttribute)
			});

			MessagingCenter.Subscribe<Page, bool> (Device.PlatformServices, Page.BusySetSignalName, OnPageBusy);

			HardwareButtons.BackPressed += OnBackPressed;

			s_isInitialized = true;
			s_state = launchActivatedEventArgs.PreviousExecutionState;
		}

		static void OnBackPressed (object sender, BackPressedEventArgs e)
		{
			Application app = Application.Current;
			if (app == null)
				return;

			Page page = app.MainPage;
			if (page == null)
				return;

			var platform = page.Platform as Platform.WinRT.Platform;
			if (platform == null)
				return;

			e.Handled = platform.BackButtonPressed ();
		}

		static ApplicationExecutionState s_state;
		static bool s_isInitialized;

		const string LogFormat = "[{0}] {1}";

		static async void OnPageBusy (Page sender, bool enabled)
		{
			StatusBar status = StatusBar.GetForCurrentView ();
			if (enabled) {
				status.ProgressIndicator.ProgressValue = null;
				await status.ProgressIndicator.ShowAsync ();
			} else
				await status.ProgressIndicator.HideAsync ();
		}

		static Windows.UI.Xaml.ResourceDictionary GetPhoneResources ()
		{
			return new Windows.UI.Xaml.ResourceDictionary {
				Source = new Uri ("ms-appx:///Xamarin.Forms.Platform.WinRT.Phone/PhoneResources.xbf")
			};
		}

		static Windows.UI.Xaml.ResourceDictionary GetResources (UserControl control)
		{
			var gresources = control.Resources.MergedDictionaries[0];
			control.Resources.MergedDictionaries.Remove (gresources);

			return gresources;
		}
	}
}