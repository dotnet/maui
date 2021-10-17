using Foundation;
using Microsoft.Maui.Platform;
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

		public override NSUserActivity? GetStateRestorationActivity(UIScene scene)
		{
			var window = Window.GetWindow();
			if (window is null)
				return null;

			var persistedState = new PersistedState();

			window.Backgrounding(persistedState);

			return persistedState.ToUserActivity(window.GetType().FullName!);
		}
	}
}