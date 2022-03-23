#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;

namespace Microsoft.Maui.Essentials
{
	public static class Platform
	{
#if ANDROID

		public static class Intent
		{
			public const string ActionAppAction = ApplicationModel.AppActionsImplementation.IntentAction;
		}

		public static Android.Content.Context AppContext => Android.App.Application.Context;

		// ActivityStateManager

		public static Android.App.Activity? CurrentActivity =>
			ActivityStateManager.Default.CurrentActivity;

		public static event EventHandler<ActivityStateChangedEventArgs>? ActivityStateChanged;

		public static Task<Android.App.Activity> WaitForActivityAsync(CancellationToken cancelToken = default) =>
			ActivityStateManager.Default.WaitForActivityAsync(cancelToken);

		public static void Init(Android.App.Application application) =>
			ActivityStateManager.Default.Init(application);

		public static void Init(Android.App.Activity activity, Android.OS.Bundle? bundle) =>
			ActivityStateManager.Default.Init(activity, bundle);

		// Permissions

		public static void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults) =>
			Permissions.Default.OnRequestPermissionsResult(requestCode, permissions, grantResults);

		// AppActions

		public static void OnNewIntent(Android.Content.Intent? intent) => 
			ApplicationModel.AppActions.Current.OnNewIntent(intent);

		public static void OnResume(Android.App.Activity? activity = null) =>
			ApplicationModel.AppActions.Current.OnResume(activity?.Intent);

#elif IOS || MACCATALYST

		public static bool OpenUrl(UIKit.UIApplication app, Foundation.NSUrl url, Foundation.NSDictionary options) =>
			Authentication.WebAuthenticator.Default.OpenUrl(new Uri(url.AbsoluteString));

		public static bool ContinueUserActivity(UIKit.UIApplication application, Foundation.NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler) =>
			Authentication.WebAuthenticator.Default.OpenUrl(new Uri(userActivity?.WebPageUrl?.AbsoluteString));

		public static void PerformActionForShortcutItem(UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler) =>
			ApplicationModel.AppActions.Current.PerformActionForShortcutItem(application, shortcutItem, completionHandler);

		public static void Init(Func<UIKit.UIViewController> getCurrentUIViewController);

		public static UIKit.UIViewController GetCurrentUIViewController();

#elif WINDOWS
		public static async void OnLaunched(UI.Xaml.LaunchActivatedEventArgs e)
		{
			if (ApplicationModel.AppActions.Current is IPlatformAppActions platform)
				await platform.OnLaunched(e);
		}
#endif
	}
}
