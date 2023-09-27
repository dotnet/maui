using System;
using Foundation;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
	public class MauiUISceneDelegate : UIResponder, IUIWindowSceneDelegate
	{
		[Export("window")]
		public virtual UIWindow? Window { get; set; }

		[Export("scene:willConnectToSession:options:")]
		public virtual void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.SceneWillConnect>(del => del(scene, session, connectionOptions));

			if (session.Configuration.Name == MauiUIApplicationDelegate.MauiSceneConfigurationKey && IPlatformApplication.Current?.Application != null)
			{
				this.CreatePlatformWindow(IPlatformApplication.Current.Application, scene, session, connectionOptions);

				if (Window != null)
					GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.OnPlatformWindowCreated>(del => del(Window));
			}
		}

		[Export("sceneDidDisconnect:")]
		public virtual void DidDisconnect(UIScene scene)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.SceneDidDisconnect>(del => del(scene));

			// for iOS 13 only where active apperance is not supported yet
			// for iOS 14+, see DidUpdateCoordinateSpace
			if (!OperatingSystem.IsMacCatalystVersionAtLeast(14))
			{
				if (Window is not null && Window.IsKeyWindow)
				{
					// manually resign the key window and rebuild the menu
					Window.ResignKeyWindow();
					UIMenuSystem
						.MainSystem
						.SetNeedsRebuild();
				}
			}
		}

		[Export("stateRestorationActivityForScene:")]
		public virtual NSUserActivity? GetStateRestorationActivity(UIScene scene)
		{
			var window = Window.GetWindow();
			if (window is null)
				return null;

			var persistedState = new PersistedState();

			window.Backgrounding(persistedState);

			// the user saved nothing, so there is nothing to restore
			if (persistedState.Count == 0)
				return null;

			return persistedState.ToUserActivity(window.GetType().FullName!);
		}

		IServiceProvider? GetServiceProvider() =>
			Window?.GetWindow()?.Handler?.GetServiceProvider();

		[Export("sceneWillEnterForeground:")]
		public virtual void WillEnterForeground(UIScene scene) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneWillEnterForeground>(del => del(scene));

		[Export("sceneDidBecomeActive:")]
		public virtual void OnActivated(UIScene scene) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneOnActivated>(del => del(scene));

		[Export("sceneWillResignActive:")]
		public virtual void OnResignActivation(UIScene scene) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneOnResignActivation>(del => del(scene));

		[Export("sceneDidEnterBackground:")]
		public virtual void DidEnterBackground(UIScene scene) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneDidEnterBackground>(del => del(scene));

		[Export("scene:openURLContexts:")]
		public virtual bool OpenUrl(UIScene scene, NSSet<UIOpenUrlContext> urlContexts)
		{
			var wasHandled = false;

			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneOpenUrl>(del =>
			{
				wasHandled = del(scene, urlContexts) || wasHandled;
			});

			return wasHandled;
		}

		[Export("scene:continueUserActivity:")]
		public virtual bool ContinueUserActivity(UIScene scene, NSUserActivity userActivity)
		{
			var wasHandled = false;

			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneContinueUserActivity>(del =>
			{
				wasHandled = del(scene, userActivity) || wasHandled;
			});

			return wasHandled;
		}

		[Export("scene:willContinueUserActivityWithType:")]
		public virtual void WillContinueUserActivity(UIScene scene, string userActivityType) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneWillContinueUserActivity>(del => del(scene, userActivityType));

		[Export("scene:didFailToContinueUserActivityWithType:error:")]
		public virtual void DidFailToContinueUserActivity(UIScene scene, string userActivityType, NSError error) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneDidFailToContinueUserActivity>(del => del(scene, userActivityType, error));

		[Export("scene:didUpdateUserActivity:")]
		public virtual void DidUpdateUserActivity(UIScene scene, NSUserActivity userActivity) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneDidUpdateUserActivity>(del => del(scene, userActivity));

		[Export("scene:restoreInteractionStateWithUserActivity:")]
		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst15.0")]
		public virtual void RestoreInteractionState(UIScene scene, NSUserActivity stateRestorationActivity) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneRestoreInteractionState>(del => del(scene, stateRestorationActivity));

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst13.0")]
		[Export("windowScene:didUpdateCoordinateSpace:interfaceOrientation:traitCollection:")]
		public virtual void DidUpdateCoordinateSpace(UIWindowScene windowScene, IUICoordinateSpace previousCoordinateSpace, UIInterfaceOrientation previousInterfaceOrientation, UITraitCollection previousTraitCollection)
		{
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.WindowSceneDidUpdateCoordinateSpace>(del => del(windowScene, previousCoordinateSpace, previousInterfaceOrientation, previousTraitCollection));

			if (OperatingSystem.IsIOSVersionAtLeast(14))
			{
				// for iOS 14+ where active apperance is supported
				var newActiveAppearance = windowScene.TraitCollection.ActiveAppearance;
				if (newActiveAppearance != previousTraitCollection.ActiveAppearance &&
					newActiveAppearance == UIUserInterfaceActiveAppearance.Active)
				{
					// if window went from inactive to active (become focused), rebuild the menu
					UIMenuSystem
						.MainSystem
						.SetNeedsRebuild();
				}
			}
		}
	}
}