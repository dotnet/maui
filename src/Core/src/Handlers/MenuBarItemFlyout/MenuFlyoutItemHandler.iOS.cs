using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler
	{
		protected override UIMenuElement CreateNativeElement()
		{
			var selector = new Selector("MenuFlyoutItemHandlerMenuClickAction:");
			var command = UICommand.Create(title: VirtualView.Text, null, selector, null);
			var menu = UIMenu.Create(VirtualView.Text, new UIMenuElement[] { command });
			return menu;
		}

		[Export("MenuFlyoutItemHandlerMenuClickAction:")]
		public void MenuClickAction(UICommand uICommand)
		{

		}
	}
}
