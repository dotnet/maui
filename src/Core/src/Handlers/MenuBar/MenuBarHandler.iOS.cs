using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuBarHandler : ElementHandler<IMenuBar, IUIMenuBuilder>, IMenuBarHandler
	{
		protected override IUIMenuBuilder CreatePlatformElement()
		{
			return MauiUIApplicationDelegate.Current.MenuBuilder
				?? throw new InvalidOperationException("Menu has not been initialized yet on the Application");
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			BuildNewMenu();
		}

		void BuildNewMenu()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			UIMenu? lastFoundMenu = null;
			foreach (var item in VirtualView)
			{
				var handler = item.ToHandler(MauiContext!);
				var menuItem = (UIMenu)handler!.PlatformView!;

				UIMenu? catalystMenu = null;

				var identifierConstant = menuItem.Identifier.GetConstant();
				if (identifierConstant != null)
				{
					catalystMenu = PlatformView.GetMenu(identifierConstant);
				}

				lastFoundMenu = catalystMenu ?? lastFoundMenu;

				if (catalystMenu == null)
				{

					if (lastFoundMenu != null)
					{
						var fileMenuId = lastFoundMenu.GetIdentifier();

						PlatformView.InsertSiblingMenuAfter(menuItem, fileMenuId);
					}
					else
					{
						PlatformView.InsertSiblingMenuBefore(menuItem, UIMenuIdentifier.File.GetConstant() ?? string.Empty);
					}
				}

				lastFoundMenu = menuItem;
			}
		}

		public void Add(IMenuBarItem view)
		{
			Rebuild();
		}

		public void Remove(IMenuBarItem view)
		{
			Rebuild();
		}

		public void Clear()
		{
			Rebuild();
		}

		public void Insert(int index, IMenuBarItem view)
		{
			Rebuild();
		}

		internal static void Rebuild()
		{
			UIMenuSystem
				.MainSystem
				.SetNeedsRebuild();
		}
	}
}
