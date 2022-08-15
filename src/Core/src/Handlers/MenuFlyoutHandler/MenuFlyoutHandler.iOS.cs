using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutHandler : ElementHandler<IMenuFlyout, UIMenu>, IMenuFlyoutHandler
	{
		protected override UIMenu CreatePlatformElement()
		{
			var platformMenu =
				VirtualView
					.ToPlatformMenu(MauiContext!);

			return platformMenu;
		}

		public void Add(IMenuElement view)
		{
			Rebuild();
		}

		public void Remove(IMenuElement view)
		{
			Rebuild();
		}

		public void Clear()
		{
			Rebuild();
		}

		public void Insert(int index, IMenuElement view)
		{
			Rebuild();
		}

		internal static void Rebuild()
		{
			// TODO: Need to figure out how to rebuild the menu items for the entire context menu. On iOS/MacCat you
			// can't add/remove individual menu items. You can call UIMenu.GetMenuByReplacingChildren(newChildren) to
			// rebuild a specific menu, but that needs to be done at the "top" (the menu itself, not individual sub-items).
			// https://github.com/dotnet/maui/issues/9359
		}
	}
}
