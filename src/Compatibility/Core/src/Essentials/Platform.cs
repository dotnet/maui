using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials
{
	public static class Platform
	{
#if ANDROID
		public static void OnNewIntent(Android.Content.Intent intent)
		{
			if (ApplicationModel.AppActions.Current is IPlatformAppActions platform)
				platform.OnNewIntent(intent);
		}

		public static void OnResume(Android.App.Activity activity = null)
		{
			if (ApplicationModel.AppActions.Current is IPlatformAppActions platform)
				platform.OnNewIntent(activity?.Intent);
		}
#elif IOS || MACCATALYST
		public static void PerformActionForShortcutItem(UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler)
		{
			if (ApplicationModel.AppActions.Current is IPlatformAppActions platform)
				platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler);
		}
#elif WINDOWS
		public static async void OnLaunched(UI.Xaml.LaunchActivatedEventArgs e)
		{
			if (ApplicationModel.AppActions.Current is IPlatformAppActions platform)
				await platform.OnLaunched(e);
		}
#endif
	}
}
