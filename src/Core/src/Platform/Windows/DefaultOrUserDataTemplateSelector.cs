using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	partial class DefaultOrUserDataTemplateSelector : DataTemplateSelector
	{
		public string? UserTemplateName { get; set; }
		public string? DefaultTemplateName { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			if (UserTemplateName != null &&
				container is FrameworkElement fe &&
				fe.Resources.TryGetValue(UserTemplateName, out object dt) &&
				dt is DataTemplate dataTemplate)
			{
				return dataTemplate;
			}

			if (UserTemplateName != null &&
				Application.Current.Resources.TryGetValue(UserTemplateName, out object appTitleBar) &&
				appTitleBar is DataTemplate appTitleBarDataTemplate)
			{
				return appTitleBarDataTemplate;
			}


			if (DefaultTemplateName != null &&
				Application.Current.Resources.TryGetValue(DefaultTemplateName, out object defaultTemplate) &&
				defaultTemplate is DataTemplate defaultDataTemplate)
				return defaultDataTemplate;

			throw new System.InvalidOperationException($"Unable to Find User Template: {UserTemplateName} or Default Template: {DefaultTemplateName}");
		}
	}
}