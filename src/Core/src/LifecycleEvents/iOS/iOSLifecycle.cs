using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class iOSLifecycle
	{
		public delegate bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler);
		public delegate void DidEnterBackground(UIApplication application);
		public delegate bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions);
		public delegate bool FinishedLaunching(UIApplication application, NSDictionary launchOptions);
		public delegate void OnActivated(UIApplication application);
		public delegate void OnResignActivation(UIApplication application);
		public delegate bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options);
		public delegate void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler);
		public delegate void WillEnterForeground(UIApplication application);
		public delegate void WillTerminate(UIApplication application);
		public delegate void ApplicationSignificantTimeChange(UIApplication application);

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