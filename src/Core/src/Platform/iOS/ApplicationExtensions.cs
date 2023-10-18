using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ApplicationExtensions
	{
		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("tvos13.0")]
		public static void RequestNewWindow(this IUIApplicationDelegate platformApplication, IApplication application, OpenWindowRequest? args)
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

		public static void CreatePlatformWindow(this IUIApplicationDelegate platformApplication, IApplication application, UIApplication uiApplication, NSDictionary launchOptions)
		{
			// Find any userinfo/dictionaries we might pass into the activation state
			var dicts = new List<NSDictionary>();
			if (uiApplication.UserActivity?.UserInfo is not null)
				dicts.Add(uiApplication.UserActivity.UserInfo);
			if (launchOptions is not null)
				dicts.Add(launchOptions);

			var window = CreatePlatformWindow(application, null, dicts.ToArray());
			if (window is not null)
			{
				platformApplication.SetWindow(window);
				platformApplication.GetWindow()?.MakeKeyAndVisible();
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("tvos13.0")]
		public static void CreatePlatformWindow(this IUIWindowSceneDelegate sceneDelegate, IApplication application, UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			// Find any userinfo/dictionaries we might pass into the activation state
			var dicts = new List<NSDictionary>();
			if (scene.UserActivity?.UserInfo is not null)
				dicts.Add(scene.UserActivity.UserInfo);
			if (session.UserInfo is not null)
				dicts.Add(session.UserInfo);
			if (session.StateRestorationActivity?.UserInfo is not null)
				dicts.Add(session.StateRestorationActivity.UserInfo);
			try
			{
				using var activities = connectionOptions.UserActivities;
				if (activities is not null)
				{
					foreach (var u in activities)
					{
						if (u is NSUserActivity userActivity && userActivity.UserInfo is not null)
							dicts.Add(userActivity.UserInfo);
					}
				}
			}
			catch (InvalidCastException)
			{
				// HACK: Workaround for https://github.com/xamarin/xamarin-macios/issues/13704
				//       This only throws if the collection is empty.
			}

			var window = CreatePlatformWindow(application, scene as UIWindowScene, dicts.ToArray());
			if (window is not null)
			{
				sceneDelegate.SetWindow(window);
				sceneDelegate.GetWindow()?.MakeKeyAndVisible();
			}
		}

		static UIWindow? CreatePlatformWindow(IApplication application, UIWindowScene? windowScene, NSDictionary[]? states)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return null;

			var uiWindow = windowScene is not null
#pragma warning disable CA1416 // UIWindow(windowScene) is only supported on: ios 13.0 and later
				? new UIWindow(windowScene)
#pragma warning restore CA1416
				: new UIWindow();

			var mauiContext = applicationContext.MakeWindowScope(uiWindow, out var windowScope);

			applicationContext.Services?.InvokeLifecycleEvents<iOSLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext, states);

			var mauiWindow = application.CreateWindow(activationState);

			uiWindow.SetWindowHandler(mauiWindow, mauiContext);

			return uiWindow;
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

		public static void UpdateUserInterfaceStyle(this IApplication application)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13) && !OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				return;

			if (application is null)
				return;

			var currentViewController = WindowStateManager.Default.GetCurrentUIViewController(false);

			if (currentViewController is null)
				return;

			switch (application.UserAppTheme)
			{
				case AppTheme.Light:
					currentViewController.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Light;
					break;
				case AppTheme.Dark:
					currentViewController.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Dark;
					break;
				default:
					currentViewController.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Unspecified;
					break;
			}
		}
	}
}