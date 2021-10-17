using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ApplicationExtensions
	{
		public static void RequestNewWindow(this UIApplicationDelegate nativeApplication, IApplication application, OpenWindowRequest? args)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext || args is null)
				return;

			var state = args?.State;
			var userActivity = state.ToUserActivity(MauiUIApplicationDelegate.MauiSceneConfigurationKey);

			UIApplication.SharedApplication.RequestSceneSessionActivation(
				null,
				userActivity,
				null,
				err => application.Handler?.MauiContext?.CreateLogger<IApplication>()?.LogError(new NSErrorException(err), err.Description));
		}

		public static NSUserActivity ToUserActivity(this IPersistedState? state, string userActivityType)
		{
			var userInfo = new NSMutableDictionary();

			if (state is not null)
			{
				foreach (var pair in state)
				{
					userInfo.SetValueForKey(new NSString(pair.Value), new NSString(pair.Key));
				}
			}

			var userActivity = new NSUserActivity(userActivityType);
			userActivity.AddUserInfoEntries(userInfo);

			return userActivity;
		}
	}
}