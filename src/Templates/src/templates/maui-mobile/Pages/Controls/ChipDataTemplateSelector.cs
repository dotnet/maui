using MauiApp._1.Models;

namespace MauiApp._1.Pages.Controls;

public class ChipDataTemplateSelector : DataTemplateSelector
{
	public required DataTemplate SelectedTagTemplate { get; set; }
	public required DataTemplate NormalTagTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		return (item as Tag)?.IsSelected ?? false ? SelectedTagTemplate : NormalTagTemplate;
	}
}