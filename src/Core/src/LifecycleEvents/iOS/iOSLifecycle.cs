using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class iOSLifecycle
	{
		public delegate bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler);
		public delegate void DidEnterBackground(UIApplication application);
		public delegate bool WillFinishLaunching(UIApplication application, NSDictionary? launchOptions);
		public delegate bool FinishedLaunching(UIApplication application, NSDictionary launchOptions);
		public delegate void OnActivated(UIApplication application);
		public delegate void OnResignActivation(UIApplication application);
		public delegate bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options);
		/// <summary>
		/// Handles a quick action delivered through the application or scene lifecycle.
		/// </summary>
		/// <param name="application">The application receiving the action.</param>
		/// <param name="shortcutItem">The user-selected quick action.</param>
		/// <param name="completionHandler">
		/// The acknowledgement callback for this registration. Invoke it exactly once with
		/// <see langword="true"/> if this handler handled the action, or <see langword="false"/> otherwise.
		/// </param>
		/// <remarks>
		/// <para>
		/// Each registration receives its own callback rather than sharing the native completion callback.
		/// Existing handlers that only observe or log the action must therefore be updated to acknowledge
		/// <see langword="false"/>. Acknowledgement may be deferred until asynchronous work completes;
		/// returning from the handler is not an acknowledgement.
		/// </para>
		/// <para>
		/// When dispatch does not throw, native completion waits until all handler invocations have returned
		/// and either one registration has reported <see langword="true"/> or every registration has reported
		/// <see langword="false"/>. If a registration never replies and none reports <see langword="true"/>,
		/// native completion can remain pending.
		/// </para>
		/// </remarks>
		public delegate void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler);
		public delegate void WillEnterForeground(UIApplication application);
		public delegate void WillTerminate(UIApplication application);
		public delegate void ApplicationSignificantTimeChange(UIApplication application);
		public delegate void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler);

		// Scene
		public delegate void SceneWillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions);
		public delegate void SceneDidDisconnect(UIScene scene);
		public delegate void SceneWillEnterForeground(UIScene scene);
		public delegate void SceneOnActivated(UIScene scene);
		public delegate void SceneOnResignActivation(UIScene scene);
		public delegate void SceneDidEnterBackground(UIScene scene);
		public delegate bool SceneOpenUrl(UIScene scene, NSSet<UIOpenUrlContext> urlContexts);
		public delegate bool SceneContinueUserActivity(UIScene scene, NSUserActivity userActivity);
		public delegate void SceneWillContinueUserActivity(UIScene scene, string userActivityType);
		public delegate void SceneDidFailToContinueUserActivity(UIScene scene, string userActivityType, NSError error);
		public delegate void SceneDidUpdateUserActivity(UIScene scene, NSUserActivity userActivity);
		public delegate void SceneRestoreInteractionState(UIScene scene, NSUserActivity stateRestorationActivity);

		// Window Scene
		public delegate void WindowSceneDidUpdateCoordinateSpace(UIWindowScene windowScene, IUICoordinateSpace previousCoordinateSpace, UIInterfaceOrientation previousInterfaceOrientation, UITraitCollection previousTraitCollection);

		// Internal events
		internal delegate void OnMauiContextCreated(IMauiContext mauiContext);
		internal delegate void OnPlatformWindowCreated(UIWindow window);
	}
}