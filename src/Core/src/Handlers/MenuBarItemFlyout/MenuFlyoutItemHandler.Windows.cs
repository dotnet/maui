using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler : MenuFlyoutItemBaseHandler<IMenuFlyoutItem, MenuFlyoutItem>
	{
		public static void MapText(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			// TODO MAUI Fix the types on interfaces
			((MenuFlyoutItem)handler.NativeView!).Text = view.Text;
		}
	}
}
