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

		/* The Selector for every single MenuElement has to be unique. If you try to reuse the same selector
		 * Then Catalyst will only ever create one MenuItem for the last menu item you added.
		 * Because we can't dynamically export these selectors we end up with this large file of premade ones.
		 * If users try to add too many menu items they will get an exception guiding them how to add their own 
		 * selectors
		 */

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem0:")]
		internal void MenuItem0(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem1:")]
		internal void MenuItem1(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem2:")]
		internal void MenuItem2(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem3:")]
		internal void MenuItem3(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem4:")]
		internal void MenuItem4(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem5:")]
		internal void MenuItem5(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem6:")]
		internal void MenuItem6(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem7:")]
		internal void MenuItem7(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem8:")]
		internal void MenuItem8(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem9:")]
		internal void MenuItem9(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem10:")]
		internal void MenuItem10(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem11:")]
		internal void MenuItem11(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem12:")]
		internal void MenuItem12(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem13:")]
		internal void MenuItem13(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem14:")]
		internal void MenuItem14(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem15:")]
		internal void MenuItem15(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem16:")]
		internal void MenuItem16(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem17:")]
		internal void MenuItem17(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem18:")]
		internal void MenuItem18(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem19:")]
		internal void MenuItem19(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem20:")]
		internal void MenuItem20(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem21:")]
		internal void MenuItem21(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem22:")]
		internal void MenuItem22(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem23:")]
		internal void MenuItem23(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem24:")]
		internal void MenuItem24(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem25:")]
		internal void MenuItem25(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem26:")]
		internal void MenuItem26(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem27:")]
		internal void MenuItem27(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem28:")]
		internal void MenuItem28(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem29:")]
		internal void MenuItem29(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem30:")]
		internal void MenuItem30(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem31:")]
		internal void MenuItem31(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem32:")]
		internal void MenuItem32(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem33:")]
		internal void MenuItem33(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem34:")]
		internal void MenuItem34(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem35:")]
		internal void MenuItem35(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem36:")]
		internal void MenuItem36(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem37:")]
		internal void MenuItem37(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem38:")]
		internal void MenuItem38(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem39:")]
		internal void MenuItem39(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem40:")]
		internal void MenuItem40(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem41:")]
		internal void MenuItem41(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem42:")]
		internal void MenuItem42(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem43:")]
		internal void MenuItem43(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem44:")]
		internal void MenuItem44(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem45:")]
		internal void MenuItem45(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem46:")]
		internal void MenuItem46(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem47:")]
		internal void MenuItem47(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem48:")]
		internal void MenuItem48(UICommand uICommand)
		{
			uICommand.SendClicked();
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("MenuItem49:")]
		internal void MenuItem49(UICommand uICommand)
		{
			uICommand.SendClicked();
		}
	}
}

