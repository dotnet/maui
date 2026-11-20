using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutItemHandler
	{
		internal static Dictionary<int, IMenuElement> menus = new Dictionary<int, IMenuElement>();

		protected override UIMenuElement CreatePlatformElement()
		{
			// https://github.com/dotnet/maui/issues/9332
			// The menu code needs to be converted over to using `UIAction`
			// so that all of this can be the same
			if (VirtualView.IsInContextFlyout())
			{
				UIImage? contextUiImage = VirtualView.Source.GetPlatformContextMenuImage(MauiContext!);

				// Menus are rebuilt every time they're requested, which disconnects this handler, while UIKit can
				// still show (and invoke) a menu it got before. The action keeps working with the item itself.
				var item = new WeakReference<IMenuFlyoutItem>(VirtualView);

				var uiAction = UIAction.Create(
					title: VirtualView.Text ?? string.Empty,
					image: contextUiImage,
					identifier: null,
					handler: (_) =>
					{
						// A menu shown before can outlive changes: MenuItem.Clicked is raised even when disabled
						if (item.TryGetTarget(out var menuItem) && menuItem.IsEnabled)
							menuItem.Clicked();
					});

				return uiAction;
			}

			return VirtualView.CreateMenuItem(MauiContext!);
		}

		public static void MapIsEnabled(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			handler.PlatformView?.UpdateIsEnabled(view);
		}

		internal static void Execute(UICommand uICommand)
		{
			if (uICommand.PropertyList is NSString nsString &&
				Int32.TryParse(nsString.ToString(), out int index))
			{
				menus[index].Clicked();
			}
		}

		internal static void Reset()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			menus.Clear();
		}
	}
}
