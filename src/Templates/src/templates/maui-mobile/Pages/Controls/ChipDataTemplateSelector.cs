using MauiApp._1.Models;

namespace MauiApp._1.Pages.Controls;

public class ChipDataTemplateSelector : DataTemplateSelector
{
	public DataTemplate? SelectedTagTemplate { get; set; }
	public DataTemplate? NormalTagTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		return ((item as Tag)?.IsSelected ?? false ? SelectedTagTemplate : NormalTagTemplate) ?? throw new InvalidOperationException("DataTemplates SelectedTagTemplate and NormalTagTemplate must be set to a non-null value.");
	}
}
