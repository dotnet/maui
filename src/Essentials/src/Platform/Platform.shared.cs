#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// A static class that contains platform-specific helper methods.
	/// </summary>
	public static class Platform
	{
#if ANDROID
		/// <summary>
		/// A static class that contains Android specific information about <see cref="Intent"/>.
		/// </summary>
		public static class Intent
		{
			/// <summary>
			/// The identifier for the <see cref="Intent"/> used by <see cref="IAppActions"/>.
			/// </summary>
			public const string ActionAppAction = AppActionsImplementation.IntentAction;
		}

		/// <summary>
		/// Gets the <see cref="Android.Content.Context"/> object that represents the current application context.
		/// </summary>
		public static Android.Content.Context AppContext => Android.App.Application.Context;

		// ActivityStateManager

		/// <summary>
		/// Gets the <see cref="Android.App.Activity"/> object that represents the application's current activity.
		/// </summary>
		public static Android.App.Activity? CurrentActivity =>
			ActivityStateManager.Default.GetCurrentActivity();

		/// <summary>
		/// Occurs when the state of an activity of this application changes.
		/// </summary>
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
		/// <summary>
		/// Gets or sets the map service API key for this platform.
		/// </summary>
		public static string? MapServiceToken
		{
			get => Geocoding.Default.GetMapServiceToken();
			set => Geocoding.Default.SetMapServiceToken(value);
		}

		public static void OnLaunched(UI.Xaml.LaunchActivatedEventArgs e) =>
			AppActions.Current.OnLaunched(e);

		public static void OnActivated(UI.Xaml.Window window, UI.Xaml.WindowActivatedEventArgs args) =>
			WindowStateManager.Default.OnActivated(window, args);

#elif TIZEN
		/// <summary>
		/// Gets a <see cref="Tizen.Applications.Package"/> object with information about the current application package.
		/// </summary>
		public static Tizen.Applications.Package CurrentPackage
		{
			get
			{
				var packageId = Tizen.Applications.Application.Current.ApplicationInfo.PackageId;
				return Tizen.Applications.PackageManager.GetPackage(packageId);
			}
		}

		/// <summary>
		/// Gets or sets the map service API key for this platform.
		/// </summary>
		public static string? MapServiceToken
		{
			get => Geocoding.Default.GetMapServiceToken();
			set => Geocoding.Default.SetMapServiceToken(value);
		}
#endif

	}
}
