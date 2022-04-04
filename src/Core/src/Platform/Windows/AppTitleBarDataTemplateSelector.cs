using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	class AppTitleBarDataTemplateSelector : DataTemplateSelector
	{
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			if (container is FrameworkElement fe && 
				fe.Resources.TryGetValue("MauiAppTitleBarTemplate", out object dt) &&
				dt is DataTemplate dataTemplate)
			{
				return dataTemplate;
			}


			if (Application.Current.Resources.TryGetValue("MauiAppTitleBarTemplate", out object appTitleBar) &&
				appTitleBar is DataTemplate appTitleBarDataTemplate)
			{
				return appTitleBarDataTemplate;
			}

			return (DataTemplate)Application.Current.Resources["MauiAppTitleBarTemplateDefault"];
		}
	}
}
