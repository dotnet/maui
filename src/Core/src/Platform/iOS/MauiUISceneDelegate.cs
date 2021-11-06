using Foundation;
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
			if (session.Configuration.Name != MauiUIApplicationDelegate.MauiSceneConfigurationKey)
				return;

			this.CreateNativeWindow(MauiUIApplicationDelegate.Current.Application, scene, session, connectionOptions);
		}

		public override void DidDisconnect(UIScene scene)
		{
			var window = (scene as UIWindowScene)?.KeyWindow?.GetWindow() ?? Window?.GetWindow();

			if (window is not null)
			{
				MauiUIApplicationDelegate.Current?.Application?.Handler?.Invoke(nameof(IApplication.OnWindowClosed), window);
			}
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