using Foundation;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUISceneDelegate : UIWindowSceneDelegate
	{
		public override UIWindow? Window { get; set; }

		public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			MauiUIApplicationDelegate.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.SceneWillConnect>(del => del(scene, session, connectionOptions));

			if (session.Configuration.Name == MauiUIApplicationDelegate.MauiSceneConfigurationKey && MauiUIApplicationDelegate.Current?.Application != null)
			{
				this.CreateNativeWindow(MauiUIApplicationDelegate.Current.Application, scene, session, connectionOptions);
			}
		}

		public override void DidDisconnect(UIScene scene)
		{
			MauiUIApplicationDelegate.Current?.Services?.InvokeLifecycleEvents<iOSLifecycle.SceneDidDisconnect>(del => del(scene));
		}

		public override NSUserActivity? GetStateRestorationActivity(UIScene scene)
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
	}
}