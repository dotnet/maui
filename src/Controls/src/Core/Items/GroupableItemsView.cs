#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A selectable items view that supports grouping of items.
	/// </summary>
	/// <remarks>
	/// <see cref="GroupableItemsView"/> extends <see cref="SelectableItemsView"/> to add support for displaying items in groups.
	/// When <see cref="IsGrouped"/> is <see langword="true"/>, items are organized into sections with optional headers and footers.
	/// Use <see cref="GroupHeaderTemplate"/> and <see cref="GroupFooterTemplate"/> to customize the appearance of group sections.
	/// </remarks>
	public class GroupableItemsView : SelectableItemsView
	{
		/// <summary>Bindable property for <see cref="IsGrouped"/>.</summary>
		public static readonly BindableProperty IsGroupedProperty =
			BindableProperty.Create(nameof(IsGrouped), typeof(bool), typeof(GroupableItemsView), false);

		/// <summary>
		/// Gets or sets a value indicating whether items should be displayed in groups.
		/// </summary>
		/// <value><see langword="true"/> if items are grouped; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		/// <remarks>
		/// When enabled, the <see cref="ItemsView.ItemsSource"/> is expected to be a collection of groups,
		/// where each group is itself a collection of items.
		/// </remarks>
		public bool IsGrouped
		{
			get => (bool)GetValue(IsGroupedProperty);
			set => SetValue(IsGroupedProperty, value);
		}

		/// <summary>Bindable property for <see cref="GroupHeaderTemplate"/>.</summary>
		public static readonly BindableProperty GroupHeaderTemplateProperty =
			BindableProperty.Create(nameof(GroupHeaderTemplate), typeof(DataTemplate), typeof(GroupableItemsView), default(DataTemplate));

		/// <summary>
		/// Gets or sets the <see cref="DataTemplate"/> used to display the header for each group.
		/// </summary>
		/// <value>A <see cref="DataTemplate"/> that defines the appearance of group headers, or <see langword="null"/> for no headers.</value>
		/// <remarks>
		/// The template's binding context is set to the group object from the <see cref="ItemsView.ItemsSource"/>.
		/// </remarks>
		public DataTemplate GroupHeaderTemplate
		{
			get => (DataTemplate)GetValue(GroupHeaderTemplateProperty);
			set => SetValue(GroupHeaderTemplateProperty, value);
		}

		/// <summary>Bindable property for <see cref="GroupFooterTemplate"/>.</summary>
		public static readonly BindableProperty GroupFooterTemplateProperty =
			BindableProperty.Create(nameof(GroupFooterTemplate), typeof(DataTemplate), typeof(GroupableItemsView), default(DataTemplate));

		/// <summary>
		/// Gets or sets the <see cref="DataTemplate"/> used to display the footer for each group.
		/// </summary>
		/// <value>A <see cref="DataTemplate"/> that defines the appearance of group footers, or <see langword="null"/> for no footers.</value>
		/// <remarks>
		/// The template's binding context is set to the group object from the <see cref="ItemsView.ItemsSource"/>.
		/// </remarks>
		public DataTemplate GroupFooterTemplate
		{
			get => (DataTemplate)GetValue(GroupFooterTemplateProperty);
			set => SetValue(GroupFooterTemplateProperty, value);
		}
	}
}
