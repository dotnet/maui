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
			var selector = new Selector(Guid.NewGuid().ToString());
			var command = UICommand.Create(title: VirtualView.Text, null, selector, null);
			return command;
		}

		[Export("MenuFlyoutItemHandlerMenuClickAction:")]
		public void MenuClickAction(UICommand uICommand)
		{

		}
	}
}
