#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	public static class Platform
	{
#if ANDROID

		public static class Intent
		{
			public const string ActionAppAction = AppActionsImplementation.IntentAction;
		}

		public static Android.Content.Context AppContext => Android.App.Application.Context;

		// ActivityStateManager

		public static Android.App.Activity? CurrentActivity =>
			ActivityStateManager.Default.GetCurrentActivity();

		public static event EventHandler<ActivityStateChangedEventArgs>? ActivityStateChanged
		{
			add => ActivityStateManager.Default.ActivityStateChanged += value;
			remove => ActivityStateManager.Default.ActivityStateChanged -= value;
		}

		public static Task<Android.App.Activity> WaitForActivityAsync(CancellationToken cancelToken = default) =>
			ActivityStateManager.Default.WaitForActivityAsync(cancelToken);

		public static void Init(Android.App.Application application) =>
			ActivityStateManager.Default.Init(application);

		public static void Init(Android.App.Activity activity, Android.OS.Bundle? bundle) =>
			ActivityStateManager.Default.Init(activity, bundle);

		// Permissions

		public static void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults) =>
			Permissions.OnRequestPermissionsResult(requestCode, permissions, grantResults);

		// AppActions

		public static void OnNewIntent(Android.Content.Intent? intent) =>
			AppActions.Current.OnNewIntent(intent);

		public static void OnResume(Android.App.Activity? activity = null) =>
			AppActions.Current.OnResume(activity?.Intent);

#elif IOS || MACCATALYST

		public static bool OpenUrl(UIKit.UIApplication app, Foundation.NSUrl url, Foundation.NSDictionary options) =>
			WebAuthenticator.Default.OpenUrl(app, url, options);

		public static bool ContinueUserActivity(UIKit.UIApplication application, Foundation.NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler) =>
			WebAuthenticator.Default.ContinueUserActivity(application, userActivity, completionHandler);

		public static void PerformActionForShortcutItem(UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler) =>
			AppActions.Current.PerformActionForShortcutItem(application, shortcutItem, completionHandler);

		public static void Init(Func<UIKit.UIViewController>? getCurrentUIViewController) =>
			WindowStateManager.Default.Init(getCurrentUIViewController);

		public static UIKit.UIViewController? GetCurrentUIViewController() =>
			WindowStateManager.Default.GetCurrentUIViewController(true);

#elif WINDOWS

		public static string? MapServiceToken
		{
			get => Geocoding.Default.GetMapServiceToken();
			set => Geocoding.Default.SetMapServiceToken(value);
		}

		public static void OnLaunched(UI.Xaml.LaunchActivatedEventArgs e) =>
			AppActions.Current.OnLaunched(e);

		public static void OnActivated(UI.Xaml.Window window, UI.Xaml.WindowActivatedEventArgs args) =>
			WindowStateManager.Default.OnActivated(window, args);

		public static void OnWindowMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) =>
			WindowStateManager.Default.OnWindowMessage(hWnd, msg, wParam, lParam);

#elif TIZEN
		public static Tizen.Applications.Package CurrentPackage
		{
			get
			{
				var packageId = Tizen.Applications.Application.Current.ApplicationInfo.PackageId;
				return Tizen.Applications.PackageManager.GetPackage(packageId);
			}
		}

		public static string? MapServiceToken
		{
			get => Geocoding.Default.GetMapServiceToken();
			set => Geocoding.Default.SetMapServiceToken(value);
		}

#endif

	}
}
