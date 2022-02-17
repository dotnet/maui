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
		static int i = 0;
		protected override UIMenuElement CreatePlatformElement()
		{
			var selector = new Selector($"MenuItem{i++}:");
			var command = UICommand.Create(title: VirtualView.Text, null, selector, null);
			return command;
		}
	}
}
