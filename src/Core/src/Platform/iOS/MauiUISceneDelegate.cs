using System;
using System.Collections.Generic;
using Foundation;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUISceneDelegate : UIWindowSceneDelegate
	{
		WeakReference<IWindow>? _virtualWindow;
		internal IWindow? VirtualWindow
		{
			get
			{
				IWindow? window = null;
				_virtualWindow?.TryGetTarget(out window);
				return window;
			}
			set
			{
				if (value != null)
				{
					if (_virtualWindow == null)
						_virtualWindow = new WeakReference<IWindow>(value);
					else
						_virtualWindow.SetTarget(value);
				}
			}
		}

		public override UIWindow? Window { get; set; }

		public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			if (session.Configuration.Name != MauiUIApplicationDelegate.MauiSceneConfigurationKey)
				return;

			if (Window == null)
			{
				var dicts = new List<NSDictionary>();

				// Find any userinfo/dictionaries we might pass into the activation state
				if (scene?.UserActivity?.UserInfo != null)
					dicts.Add(scene.UserActivity.UserInfo);
				if (session.UserInfo != null)
					dicts.Add(session.UserInfo);
				if (session.StateRestorationActivity?.UserInfo != null)
					dicts.Add(session.StateRestorationActivity.UserInfo);
				if (connectionOptions.UserActivities != null)
				{
					foreach (var u in connectionOptions.UserActivities)
					{
						if (u is NSUserActivity userActivity && userActivity.UserInfo != null)
							dicts.Add(userActivity.UserInfo);
					}
				}

				var w = MauiUIApplicationDelegate.Current.CreateNativeWindow(scene as UIWindowScene, dicts.ToArray());
				Window = w.nativeWIndow;
				VirtualWindow = w.virtualWindow;
			}

			Window?.MakeKeyAndVisible();
		}

		public override NSUserActivity? GetStateRestorationActivity(UIScene scene)
		{
			var virtualWindow = MauiUIApplicationDelegate.Current.VirtualWindow;

			if (virtualWindow == null)
			{
				Console.WriteLine("VirtualWindow null, no state to save...");
				return null;
			}

			var persistedState = new PersistedState();

			virtualWindow.Backgrounding(persistedState);

			return persistedState.ToUserActivity(virtualWindow.GetType().FullName!);
		}
	}
}
