using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WMenuItem = System.Windows.Controls.MenuItem;
using WApplication = System.Windows.Application;

namespace Xamarin.Forms.Platform.WPF
{
	public sealed class CustomContextMenu : ContextMenu
	{
		protected override DependencyObject GetContainerForItemOverride()
		{
			var item = new WMenuItem();
			item.SetBinding(HeaderedItemsControl.HeaderProperty, new System.Windows.Data.Binding("Text"));

			item.Click += (sender, args) =>
			{
				IsOpen = false;

				var menuItem = item.DataContext as MenuItem;
				if (menuItem != null)
					((IMenuItemController)menuItem).Activate();
			};
			return item;
		}
	}
}
