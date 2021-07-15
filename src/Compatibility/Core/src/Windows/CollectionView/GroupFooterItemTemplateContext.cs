using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class GroupFooterItemTemplateContext : ItemTemplateContext
	{
		public GroupFooterItemTemplateContext(DataTemplate formsDataTemplate, object item, 
			BindableObject container, double? height = null, double? width = null, Thickness? itemSpacing = null) 
			: base(formsDataTemplate, item, container, height, width, itemSpacing)
		{
		}

		public static void EnsureSelectionDisabled(DependencyObject element, object item)
		{
			if (item is GroupFooterItemTemplateContext)
			{
				// Prevent the group footer from being selectable
				(element as FrameworkElement).IsHitTestVisible = false;
			}

		}
	}
}