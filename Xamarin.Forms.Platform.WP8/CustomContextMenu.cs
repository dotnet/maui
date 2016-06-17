using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using WMenuItem = Microsoft.Phone.Controls.MenuItem;
using WApplication = System.Windows.Application;

namespace Xamarin.Forms.Platform.WinPhone
{
	public sealed class CustomContextMenu : ContextMenu
	{
		protected override DependencyObject GetContainerForItemOverride()
		{
			var item = new WMenuItem();
			item.SetBinding(HeaderedItemsControl.HeaderProperty, new System.Windows.Data.Binding("Text") { Converter = (System.Windows.Data.IValueConverter)WApplication.Current.Resources["LowerConverter"] });

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