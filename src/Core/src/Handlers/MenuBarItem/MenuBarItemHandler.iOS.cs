using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, UIMenu>, IMenuBarItemHandler
	{
		protected override UIMenu CreateNativeElement()
		{
			UIMenu? catalystMenu = null;
			if (Enum.TryParse(typeof(UIMenuIdentifier), VirtualView.Text, out object? result))
			{
				if (result != null)
				{
					catalystMenu =
						MauiUIApplicationDelegate.Current.MenuBuilder?.GetMenu(((UIMenuIdentifier)result).GetConstant());
				}
			}

			UIMenuElement[] menuElements = new UIMenuElement[VirtualView.Count];
			for (int i = 0; i < VirtualView.Count; i++)
			{
				var item = VirtualView[i];
				var menuElement = (UIMenuElement)item.ToHandler(MauiContext!)!.NativeView!;
				menuElements[i] = menuElement;
			}

			if (catalystMenu != null)
			{
				var menuContainer =
					UIMenu.Create(String.Empty, null,
						UIMenuIdentifier.None,
						UIMenuOptions.DisplayInline,
						menuElements);

				MauiUIApplicationDelegate
								.Current
								.MenuBuilder?
								.InsertChildMenuAtStart(menuContainer, catalystMenu.GetIdentifier());
			}
			else
			{
				catalystMenu =
					UIMenu.Create(VirtualView.Text, null, UIMenuIdentifier.None,
						UIMenuOptions.SingleSelection, menuElements);
			}

			return catalystMenu;
		}



		[Export("MenuBarItemHandlerMenuClickAction:")]
		public void MenuClickAction(UICommand uICommand)
		{

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
			UIMenuSystem
						.MainSystem
						.SetNeedsRebuild();
		}
	}
}
