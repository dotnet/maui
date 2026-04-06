using UWPApp = Microsoft.UI.Xaml.Application;
using UWPDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WinDataTemplateSelector = Microsoft.UI.Xaml.Controls.DataTemplateSelector;

namespace Microsoft.Maui.Controls.Platform;

// Selects ItemsViewDefaultTemplate for fake group-footer items (GroupFooterItemTemplateContext)
// so that the GroupFooterTemplate is applied when no ItemTemplate is set on the CollectionView.
// All other items return null, letting WinUI fall back to its default ToString() rendering.
internal partial class GroupFooterDataTemplateSelector : WinDataTemplateSelector
{
	readonly UWPDataTemplate? _footerTemplate;

	public GroupFooterDataTemplateSelector()
	{
		_footerTemplate = UWPApp.Current.Resources["ItemsViewDefaultTemplate"] as UWPDataTemplate;
	}

	protected override UWPDataTemplate SelectTemplateCore(object item)
	{
		if (item is GroupFooterItemTemplateContext)
		{
			return _footerTemplate!;
		}

		return null!;
	}

	protected override UWPDataTemplate SelectTemplateCore(object item, Microsoft.UI.Xaml.DependencyObject container)
	{
		return SelectTemplateCore(item);
	}
}
