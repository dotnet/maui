using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public partial class MauiUIApplicationDelegate
	{
		internal IUIMenuBuilder? MenuBuilder { get; private set; }

		[SupportedOSPlatform("ios13.0")]
		public override void BuildMenu(IUIMenuBuilder builder)
		{
			MenuFlyoutItemHandler.Reset();

			base.BuildMenu(builder);

			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			MenuBuilder = builder;

			UIWindow? window = null;
			if (OperatingSystem.IsMacCatalystVersionAtLeast(14))
			{
				// for iOS 14+ where active apperance is supported
				var activeWindowScenes = new List<UIWindowScene>();
				foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
				{
					if (scene is UIWindowScene windowScene &&
						windowScene.TraitCollection.ActiveAppearance == UIUserInterfaceActiveAppearance.Active)
					{
						activeWindowScenes.Add(windowScene);
					}
				}

				if (activeWindowScenes.Count > 0)
				{
					// when a new window is created, some time more than 1 active window sence are returned
					// we need to pick the newly created window in this case
					// the order of window scene returned is not trustable, do not use last
					// after some manual testing, windowing behaviour that is not ready yet is the newly created window
					if (activeWindowScenes.Count > 1)
					{
						foreach (var ws in activeWindowScenes)
						{
							if (ws.WindowingBehaviors is not null && !ws.WindowingBehaviors.Closable)
							{
								window = ws.KeyWindow;
								break;
							}
						}
					}
					else
						window = activeWindowScenes[0].KeyWindow;
				}
			}
			else
			{
				// for iOS 13 where active apperance is not supported yet
				window = Window ?? this.GetWindow() ??
					UIApplication.SharedApplication.GetWindow()?.Handler?.PlatformView as UIWindow;
			}
			window?.GetWindow()?.Handler?.UpdateValue(nameof(IMenuBarElement.MenuBar));

			MenuBuilder = null;
		}

		public override bool CanPerform(Selector action, NSObject? withSender)
		{
			if (action.Name.StartsWith("MenuItem", StringComparison.Ordinal))
				return true;

			return base.CanPerform(action, withSender);
		}

		[SupportedOSPlatform("ios13.0")]
		[Export(AcceleratorExtensions.MenuItemSelectedSelector)]
		internal void MenuItemSelected(UICommand uiCommand)
		{
			uiCommand.SendClicked();
		}
	}
}

