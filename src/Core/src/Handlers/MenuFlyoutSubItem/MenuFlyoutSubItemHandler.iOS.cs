using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using ObjCRuntime;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSubItemHandler
	{
		protected override UIMenu CreateNativeElement()
		{
			var selector = new Selector("MenuFlyoutSubItemHandlerMenuClickAction:");
			var command = UICommand.Create(title: VirtualView.Text, null, selector, null);
			var menu = UIMenu.Create(VirtualView.Text, new UIMenuElement[] { command });
			return menu;
		}

		[Export("MenuFlyoutSubItemHandlerMenuClickAction:")]
		public void MenuClickAction(UICommand uICommand)
		{

		}

		public void Add(IMenuElement view)
		{
		}

		public void Remove(IMenuElement view)
		{
		}

		public void Clear()
		{
		}

		public void Insert(int index, IMenuElement view)
		{
		}
	}
}
