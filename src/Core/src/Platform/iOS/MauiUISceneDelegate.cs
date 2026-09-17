using System;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
		[UnconditionalSuppressMessage("Memory", "MEM0002",
			Justification = "Retains shortcut metadata until activation, with no scene/delegate back-reference. Cleared on activation, disconnect, initialization failure, or reconnect.")]
		UIApplicationShortcutItem? _pendingShortcutItem;

		[Export("window")]
		public virtual UIWindow? Window { get; set; }

		[Export("scene:willConnectToSession:options:")]
		public virtual void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			_pendingShortcutItem = null;
			var connected = false;

			try
			{
				_pendingShortcutItem = connectionOptions.ShortcutItem;
				IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.SceneWillConnect>(del => del(scene, session, connectionOptions));

				if (session.Configuration.Name == MauiUIApplicationDelegate.MauiSceneConfigurationKey && IPlatformApplication.Current?.Application != null)
				{
					this.CreatePlatformWindow(IPlatformApplication.Current.Application, scene, session, connectionOptions);

					if (Window is UIWindow window && GetServiceProvider() is IServiceProvider services)
					{
						services.InvokeLifecycleEvents<iOSLifecycle.OnPlatformWindowCreated>(del => del(window));
						connected = true;
					}
				}

				if (!connected && _pendingShortcutItem is UIApplicationShortcutItem shortcutItem)
					LogUnavailableWindow(shortcutItem);
			}
			finally
			{
				if (!connected)
					_pendingShortcutItem = null;
			}
		}

		[Export("sceneDidDisconnect:")]
		public virtual void DidDisconnect(UIScene scene)
		{
			_pendingShortcutItem = null;
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

		// The application provider is only used for diagnostics, never to route another window's action.
		static void LogUnavailableWindow(UIApplicationShortcutItem shortcutItem) =>
			IPlatformApplication.Current?.Services?
				.GetService<ILoggerFactory>()?
				.CreateLogger<MauiUISceneDelegate>()
				.LogWarning("Unable to dispatch shortcut '{ShortcutType}' because its MAUI window is unavailable.", shortcutItem.Type);

		// A cold scene connection supplies no native completion handler.
		static void IgnoreColdShortcutCompletion(bool _)
		{
		}

		[Export("sceneWillEnterForeground:")]
		public virtual void WillEnterForeground(UIScene scene) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneWillEnterForeground>(del => del(scene));

		[Export("sceneDidBecomeActive:")]
		public virtual void OnActivated(UIScene scene)
		{
			var shortcutItem = _pendingShortcutItem;
			_pendingShortcutItem = null;
			var window = Window;

			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneOnActivated>(del => del(scene));

			if (shortcutItem is null)
				return;

			var services = ReferenceEquals(window, Window) ? GetServiceProvider() : null;
			if (services is null)
			{
				LogUnavailableWindow(shortcutItem);
				return;
			}

			services.DispatchShortcutItem(UIApplication.SharedApplication, shortcutItem, IgnoreColdShortcutCompletion);
		}

		[Export("sceneWillResignActive:")]
		public virtual void OnResignActivation(UIScene scene) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneOnResignActivation>(del => del(scene));

		[Export("sceneDidEnterBackground:")]
		public virtual void DidEnterBackground(UIScene scene) =>
			GetServiceProvider()?.InvokeLifecycleEvents<iOSLifecycle.SceneDidEnterBackground>(del => del(scene));

		/// <summary>
		/// Handles a quick action delivered to an existing window scene.
		/// </summary>
		/// <param name="windowScene">The scene receiving the action.</param>
		/// <param name="shortcutItem">The user-selected quick action.</param>
		/// <param name="completionHandler">Called once on the main thread to report whether the action was handled.</param>
		/// <remarks>
		/// Dispatches through the existing <see cref="iOSLifecycle.PerformActionForShortcutItem"/> registrations.
		/// </remarks>
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst13.1")]
		[Export("windowScene:performActionForShortcutItem:completionHandler:")]
		// Match the protocol method name and delegate type for native block marshalling.
		public virtual void PerformAction(UIWindowScene windowScene, UIApplicationShortcutItem shortcutItem, Action<bool> completionHandler)
		{
			var services = GetServiceProvider();
			services.DispatchShortcutItem(UIApplication.SharedApplication, shortcutItem, completionHandler.Invoke);

			if (services is null)
				LogUnavailableWindow(shortcutItem);
		}

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