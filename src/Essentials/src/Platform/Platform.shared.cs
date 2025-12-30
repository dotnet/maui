#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices.Sensors;
#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
#endif

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
		/// Gets the <see cref="Context"/> object that represents the current application context.
		/// </summary>
		public static Context AppContext => Application.Context;

		// ActivityStateManager

		/// <inheritdoc cref="IActivityStateManager.GetCurrentActivity"/>
		public static Activity? CurrentActivity =>
			ActivityStateManager.Default.GetCurrentActivity();


		/// <inheritdoc cref="IActivityStateManager.ActivityStateChanged"/>
		public static event EventHandler<ActivityStateChangedEventArgs>? ActivityStateChanged
		{
			add => ActivityStateManager.Default.ActivityStateChanged += value;
			remove => ActivityStateManager.Default.ActivityStateChanged -= value;
		}

		/// <inheritdoc cref="IActivityStateManager.WaitForActivityAsync(CancellationToken)"/>
		public static Task<Activity> WaitForActivityAsync(CancellationToken cancelToken = default) =>
			ActivityStateManager.Default.WaitForActivityAsync(cancelToken);

		/// <inheritdoc cref="IActivityStateManager.Init(Application)"/>
		public static void Init(Application application) =>
			ActivityStateManager.Default.Init(application);

		/// <inheritdoc cref="IActivityStateManager.Init(Activity, global::Android.OS.Bundle?)"/>
		public static void Init(Activity activity, global::Android.OS.Bundle? bundle) =>
			ActivityStateManager.Default.Init(activity, bundle);

		// Permissions

		/// <summary>
		/// Pass permission request results from an activity's overridden method to the library for handling internal permission requests.
		/// </summary>
		/// <param name="requestCode">The requestCode from the corresponding overridden method in an activity.</param>
		/// <param name="permissions">The permissions from the corresponding overridden method in an activity.</param>
		/// <param name="grantResults">The grantResults from the corresponding overridden method in an activity.</param>
		public static void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) =>
			Permissions.OnRequestPermissionsResult(requestCode, permissions, grantResults);

		// AppActions

		/// <summary>
		/// Called when a new <see cref="Intent"/> was created as part of invoking an app action.
		/// </summary>
		/// <param name="intent">The <see cref="Intent"/> that is created.</param>
		public static void OnNewIntent(global::Android.Content.Intent? intent) =>
			AppActions.Current.OnNewIntent(intent);

		/// <summary>
		/// Called when a <see cref="Activity"/> is resumed as part of invoking an app action.
		/// </summary>
		/// <param name="activity">The <see cref="Activity"/> that is resumed.</param>
		public static void OnResume(Activity? activity = null) =>
			AppActions.Current.OnResume(activity?.Intent);

#elif IOS || MACCATALYST
		/// <summary>
		/// Opens the specified URI to start a authentication flow.
		/// </summary>
		/// <param name="app">This parameters is not used.</param>
		/// <param name="url">The URL to open that will start the authentication flow.</param>
		/// <param name="options">This parameters is not used.</param>
		/// <returns><see langword="true"/> when the URI has been opened, otherwise <see langword="false"/>.</returns>
		public static bool OpenUrl(UIKit.UIApplication app, Foundation.NSUrl url, Foundation.NSDictionary options) =>
			WebAuthenticator.Default.OpenUrl(app, url, options);

		/// <summary>
		/// Informs the app that there is data associated with continuing a task specified as a <see cref="Foundation.NSUserActivity"/> object, and then returns whether the app continued the activity.
		/// </summary>
		/// <param name="application">The application this action is invoked on.</param>
		/// <param name="userActivity">The user activity identifier.</param>
		/// <param name="completionHandler">The action that is invoked when the operation is completed.</param>
		/// <returns><see langword="true"/> if the app handled the user activity, otherwise <see langword="false"/>.</returns>
		public static bool ContinueUserActivity(UIKit.UIApplication application, Foundation.NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler) =>
			WebAuthenticator.Default.ContinueUserActivity(application, userActivity, completionHandler);

		/// <summary>
		/// Invokes the action that corresponds to the chosen <see cref="AppAction"/> by the user.
		/// </summary>
		/// <param name="application">The application this action is invoked on.</param>
		/// <param name="shortcutItem">The shortcut item that was chosen by the user.</param>
		/// <param name="completionHandler">The action that is invoked when the operation is completed.</param>
		public static void PerformActionForShortcutItem(UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler) =>
			AppActions.Current.PerformActionForShortcutItem(application, shortcutItem, completionHandler);

		/// <summary>
		/// Initializes the <see cref="WindowStateManager"/> for the given <see cref="UIKit.UIViewController"/>.
		/// </summary>
		/// <param name="getCurrentUIViewController">The <see cref="UIKit.UIViewController"/> to use for initialization.</param>
		public static void Init(Func<UIKit.UIViewController>? getCurrentUIViewController) =>
			WindowStateManager.Default.Init(getCurrentUIViewController);

		/// <summary>
		/// Gets the current view controller through the <see cref="WindowStateManager"/>.
		/// </summary>
		/// <returns>The <see cref="UIKit.UIViewController"/> object that is currently presented.</returns>
		/// <exception cref="NullReferenceException">Thrown if no current <see cref="UIKit.UIViewController"/> can be found.</exception>
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

		/// <inheritdoc cref="IPlatformAppActions.OnLaunched(UI.Xaml.LaunchActivatedEventArgs)"/>
		public static void OnLaunched(UI.Xaml.LaunchActivatedEventArgs e) =>
			AppActions.Current.OnLaunched(e);

		/// <inheritdoc cref="IWindowStateManager.OnPlatformWindowInitialized(UI.Xaml.Window)"/>
		public static void OnPlatformWindowInitialized(UI.Xaml.Window window) =>
			WindowStateManager.Default.OnPlatformWindowInitialized(window);

		/// <inheritdoc cref="IWindowStateManager.OnActivated(UI.Xaml.Window, UI.Xaml.WindowActivatedEventArgs)"/>
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
