using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutSubItemHandler
	{
		protected override UIMenu CreatePlatformElement()
		{
			// Context menus are standalone menus: they must not be merged into (or require) the main menu bar
			if (VirtualView.IsInContextFlyout())
			{
				return UIMenu.Create(
					VirtualView.Text ?? string.Empty,
					VirtualView.Source.GetPlatformContextMenuImage(MauiContext!),
					UIMenuIdentifier.None,
					0,
					VirtualView.ToPlatformContextMenuElements(MauiContext!));
			}

			var menuBar = VirtualView.FindParentOfType<IMenuBar>();

			IUIMenuBuilder? uIMenuBuilder = null;
			if (menuBar?.Handler?.PlatformView is IUIMenuBuilder builder)
			{
				uIMenuBuilder = builder;
			}

			var menu =
				VirtualView.ToPlatformMenu(
					VirtualView.Text,
					VirtualView.Source,
					MauiContext!,
					uIMenuBuilder);

			return menu;
		}

		public static void MapIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler?.PlatformView?.UpdateIsEnabled(view);
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

		void Rebuild()
		{
			// Context menus are rebuilt every time they open, so only the main menu needs a rebuild
			if (VirtualView.IsInContextFlyout())
				return;

			MenuBarHandler.Rebuild();
		}
	}
}
