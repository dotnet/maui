namespace Xamarin.Forms
{
	public class GroupableItemsView : SelectableItemsView
	{
		public static readonly BindableProperty IsGroupedProperty =
			BindableProperty.Create(nameof(IsGrouped), typeof(bool), typeof(GroupableItemsView), false);

		public bool IsGrouped
		{
			get => (bool)GetValue(IsGroupedProperty);
			set => SetValue(IsGroupedProperty, value);
		}

		public static readonly BindableProperty GroupHeaderTemplateProperty =
			BindableProperty.Create(nameof(GroupHeaderTemplate), typeof(DataTemplate), typeof(GroupableItemsView), default(DataTemplate));

		public DataTemplate GroupHeaderTemplate
		{
			get => (DataTemplate)GetValue(GroupHeaderTemplateProperty);
			set => SetValue(GroupHeaderTemplateProperty, value);
		}

		public static readonly BindableProperty GroupFooterTemplateProperty =
			BindableProperty.Create(nameof(GroupFooterTemplate), typeof(DataTemplate), typeof(GroupableItemsView), default(DataTemplate));

		public DataTemplate GroupFooterTemplate
		{
			get => (DataTemplate)GetValue(GroupFooterTemplateProperty);
			set => SetValue(GroupFooterTemplateProperty, value);
		}
	}
}