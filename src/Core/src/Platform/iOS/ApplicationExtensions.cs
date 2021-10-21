using System.Collections.Generic;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ApplicationExtensions
	{
		public static void CreateNativeWindow(this UIApplicationDelegate nativeApplication, IApplication application, UIApplication uiApplication, NSDictionary launchOptions)
		{
			var window = CreateNativeWindow(application, null);
			if (window is not null)
			{
				nativeApplication.Window = window;
				nativeApplication.Window.MakeKeyAndVisible();
			}
		}

		public static void CreateNativeWindow(this UIWindowSceneDelegate sceneDelegate, IApplication application, UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			var window = CreateNativeWindow(application, scene as UIWindowScene);
			if (window is not null)
			{
				sceneDelegate.Window = window;
				sceneDelegate.Window.MakeKeyAndVisible();
			}
		}

		static UIWindow? CreateNativeWindow(IApplication application, UIWindowScene? windowScene)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return null;

			var uiWindow = windowScene is not null
				? new UIWindow(windowScene)
				: new UIWindow();

			var mauiContext = applicationContext.MakeWindowScope(uiWindow, out var windowScope);

			applicationContext.Services?.InvokeLifecycleEvents<iOSLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext);

			var mauiWindow = application.CreateWindow(activationState);

			uiWindow.SetWindowHandler(mauiWindow, mauiContext);

			return uiWindow;
		}
	}
}