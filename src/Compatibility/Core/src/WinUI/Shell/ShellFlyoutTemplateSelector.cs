using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class ShellFlyoutTemplateSelector : Microsoft.UI.Xaml.Controls.DataTemplateSelector
	{
		Microsoft.UI.Xaml.DataTemplate BaseShellItemTemplate { get; }
		Microsoft.UI.Xaml.DataTemplate MenuItemTemplate { get; }
		Microsoft.UI.Xaml.DataTemplate SeperatorTemplate { get; }

		public ShellFlyoutTemplateSelector()
		{
			BaseShellItemTemplate = (Microsoft.UI.Xaml.DataTemplate)Microsoft.UI.Xaml.Application.Current.Resources["ShellFlyoutBaseShellItemTemplate"];
			MenuItemTemplate = (Microsoft.UI.Xaml.DataTemplate)Microsoft.UI.Xaml.Application.Current.Resources["ShellFlyoutMenuItemTemplate"];
			SeperatorTemplate = (Microsoft.UI.Xaml.DataTemplate)Microsoft.UI.Xaml.Application.Current.Resources["ShellFlyoutSeperatorTemplate"];
		}

		protected override Microsoft.UI.Xaml.DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}

		protected override Microsoft.UI.Xaml.DataTemplate SelectTemplateCore(object item)
		{
			if (item is MenuFlyoutSeparator)
				return SeperatorTemplate;
				
			if (item is MenuItem)
				return MenuItemTemplate;
			
			return BaseShellItemTemplate;
		}
	}
}
