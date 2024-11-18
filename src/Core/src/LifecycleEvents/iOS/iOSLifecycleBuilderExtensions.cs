using Foundation;
using UIKit;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class iOSLifecycleBuilderExtensions
	{
		public static IiOSLifecycleBuilder ContinueUserActivity(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.ContinueUserActivity del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder DidEnterBackground(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.DidEnterBackground del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder WillFinishLaunching(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.WillFinishLaunching del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder FinishedLaunching(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.FinishedLaunching del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder OnActivated(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.OnActivated del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder OnResignActivation(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.OnResignActivation del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder OpenUrl(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.OpenUrl del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder PerformActionForShortcutItem(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.PerformActionForShortcutItem del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder WillEnterForeground(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.WillEnterForeground del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder WillTerminate(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.WillTerminate del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder ApplicationSignificantTimeChange(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.ApplicationSignificantTimeChange del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneWillConnect(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneWillConnect del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneDidDisconnect(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneDidDisconnect del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneOnActivated(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneOnActivated del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneOnResignActivation(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneOnResignActivation del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneWillEnterForeground(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneWillEnterForeground del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneDidEnterBackground(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneDidEnterBackground del) => lifecycle.OnEvent(del);


		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneOpenUrl(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneOpenUrl del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneContinueUserActivity(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneContinueUserActivity del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneWillContinueUserActivity(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneWillContinueUserActivity del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneDidFailToContinueUserActivity(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneDidFailToContinueUserActivity del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public static IiOSLifecycleBuilder SceneDidUpdateUserActivity(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneDidUpdateUserActivity del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst15.0")]
		public static IiOSLifecycleBuilder SceneRestoreInteractionState(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.SceneRestoreInteractionState del) => lifecycle.OnEvent(del);

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst13.0")]
		public static IiOSLifecycleBuilder WindowSceneDidUpdateCoordinateSpace(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.WindowSceneDidUpdateCoordinateSpace del) => lifecycle.OnEvent(del);

		internal static IiOSLifecycleBuilder OnMauiContextCreated(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.OnMauiContextCreated del) => lifecycle.OnEvent(del);
		internal static IiOSLifecycleBuilder OnPlatformWindowCreated(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.OnPlatformWindowCreated del) => lifecycle.OnEvent(del);
	}
}