using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, UIMenu>, IMenuBarItemHandler
	{
		protected override UIMenu CreateNativeElement()
		{
			UIMenuElement[] menuElements = new UIMenuElement[VirtualView.Count];

			for(int i = 0; i < VirtualView.Count; i++) 
			{
				var item = VirtualView[i];
				var menuElement = (UIMenuElement)item.ToHandler(MauiContext!)!.NativeView!;
				menuElements[i] = menuElement;
			}

			return UIMenu.Create(VirtualView.Text, null, UIMenuIdentifier.None,
				UIMenuOptions.DisplayInline, menuElements);
		}

		/*public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in ((IMenuBarItem)view))
			{
				Add(item);
			}
		}*/

		public void Add(IMenuElement view)
		{
			//NativeView.c
			//var handler = view.ToHandler(MauiContext!);
			//var menuItem = (UIMenuElement)handler!.NativeView!;

			//NativeView.Items.Add((MenuBarItem)view.ToPlatform(MauiContext!));
		}

		public void Remove(IMenuElement view)
		{
		}

		public void Clear()
		{
			//NativeView.Items.Clear();
		}

		public void Insert(int index, IMenuElement view)
		{
		}
	}
}
