namespace Xamarin.Forms.Platform.UWP
{
	public class ShellFlyoutTemplateSelector : Windows.UI.Xaml.Controls.DataTemplateSelector
	{
		Windows.UI.Xaml.DataTemplate BaseShellItemTemplate { get; }
		Windows.UI.Xaml.DataTemplate MenuItemTemplate { get; }
		Windows.UI.Xaml.DataTemplate SeperatorTemplate { get; }

		public ShellFlyoutTemplateSelector()
		{
			BaseShellItemTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["ShellFlyoutBaseShellItemTemplate"];
			MenuItemTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["ShellFlyoutMenuItemTemplate"];
			SeperatorTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["ShellFlyoutSeperatorTemplate"];
		}

		protected override Windows.UI.Xaml.DataTemplate SelectTemplateCore(object item)
		{
			if (item is BaseShellItem)
				return BaseShellItemTemplate;
			if (item is MenuItem)
				return MenuItemTemplate;
			if (item == null)
				return SeperatorTemplate;
			return base.SelectTemplateCore(item);
		}
	}
}
