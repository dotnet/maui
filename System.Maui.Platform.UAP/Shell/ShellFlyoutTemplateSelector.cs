using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;

namespace System.Maui.Platform.UWP
{
	public class ShellFlyoutTemplateSelector : global::Windows.UI.Xaml.Controls.DataTemplateSelector
	{
		global::Windows.UI.Xaml.DataTemplate BaseShellItemTemplate { get; }
		global::Windows.UI.Xaml.DataTemplate MenuItemTemplate { get; }
		global::Windows.UI.Xaml.DataTemplate SeperatorTemplate { get; }

		public ShellFlyoutTemplateSelector()
		{
			BaseShellItemTemplate = (global::Windows.UI.Xaml.DataTemplate)global::Windows.UI.Xaml.Application.Current.Resources["ShellFlyoutBaseShellItemTemplate"];
			MenuItemTemplate = (global::Windows.UI.Xaml.DataTemplate)global::Windows.UI.Xaml.Application.Current.Resources["ShellFlyoutMenuItemTemplate"];
			SeperatorTemplate = (global::Windows.UI.Xaml.DataTemplate)global::Windows.UI.Xaml.Application.Current.Resources["ShellFlyoutSeperatorTemplate"];
		}

		protected override global::Windows.UI.Xaml.DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}

		protected override global::Windows.UI.Xaml.DataTemplate SelectTemplateCore(object item)
		{
			if (item is MenuFlyoutSeparator)
				return SeperatorTemplate;
				
			if (item is MenuItem)
				return MenuItemTemplate;
			
			return BaseShellItemTemplate;
		}
	}
}
