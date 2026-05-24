#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// An items view that supports headers, footers, and configurable item layouts.
	/// </summary>
	/// <remarks>
	/// <see cref="StructuredItemsView"/> extends <see cref="ItemsView"/> to add structural elements like headers and footers,
	/// as well as control over how items are laid out through the <see cref="ItemsLayout"/> property.
	/// This class serves as the base for views like <see cref="SelectableItemsView"/> and ultimately <see cref="CollectionView"/>.
	/// </remarks>
	public class StructuredItemsView : ItemsView
	{
		/// <summary>Bindable property for <see cref="Header"/>.</summary>
		public static readonly BindableProperty HeaderProperty =
			BindableProperty.Create(nameof(Header), typeof(object), typeof(ItemsView), null);

		/// <summary>
		/// Gets or sets the object to display as the header of the items view.
		/// </summary>
		/// <value>The header content, which can be a string, view, or any object that will be rendered using <see cref="HeaderTemplate"/> if provided.</value>
		/// <remarks>
		/// The header appears before all items in the collection.
		/// If <see cref="HeaderTemplate"/> is set, it is used to render the header; otherwise, the object's string representation is displayed.
		/// </remarks>
		public object Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		/// <summary>Bindable property for <see cref="HeaderTemplate"/>.</summary>
		public static readonly BindableProperty HeaderTemplateProperty =
			BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		/// <summary>
		/// Gets or sets the <see cref="DataTemplate"/> used to render the header.
		/// </summary>
		/// <value>A <see cref="DataTemplate"/> that defines how the header is displayed, or <see langword="null"/> to use the default rendering.</value>
		/// <remarks>
		/// The template's binding context is set to the <see cref="Header"/> object.
		/// </remarks>
		public DataTemplate HeaderTemplate
		{
			get => (DataTemplate)GetValue(HeaderTemplateProperty);
			set => SetValue(HeaderTemplateProperty, value);
		}

		/// <summary>Bindable property for <see cref="Footer"/>.</summary>
		public static readonly BindableProperty FooterProperty =
			BindableProperty.Create(nameof(Footer), typeof(object), typeof(ItemsView), null);

		/// <summary>
		/// Gets or sets the object to display as the footer of the items view.
		/// </summary>
		/// <value>The footer content, which can be a string, view, or any object that will be rendered using <see cref="FooterTemplate"/> if provided.</value>
		/// <remarks>
		/// The footer appears after all items in the collection.
		/// If <see cref="FooterTemplate"/> is set, it is used to render the footer; otherwise, the object's string representation is displayed.
		/// </remarks>
		public object Footer
		{
			get => GetValue(FooterProperty);
			set => SetValue(FooterProperty, value);
		}

		/// <summary>Bindable property for <see cref="FooterTemplate"/>.</summary>
		public static readonly BindableProperty FooterTemplateProperty =
			BindableProperty.Create(nameof(FooterTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		/// <summary>
		/// Gets or sets the <see cref="DataTemplate"/> used to render the footer.
		/// </summary>
		/// <value>A <see cref="DataTemplate"/> that defines how the footer is displayed, or <see langword="null"/> to use the default rendering.</value>
		/// <remarks>
		/// The template's binding context is set to the <see cref="Footer"/> object.
		/// </remarks>
		public DataTemplate FooterTemplate
		{
			get => (DataTemplate)GetValue(FooterTemplateProperty);
			set => SetValue(FooterTemplateProperty, value);
		}

		/// <summary>Bindable property for <see cref="ItemsLayout"/>.</summary>
		public static readonly BindableProperty ItemsLayoutProperty = InternalItemsLayoutProperty;

		/// <summary>
		/// Gets or sets the layout strategy used to arrange items in the view.
		/// </summary>
		/// <value>An <see cref="IItemsLayout"/> that defines how items are arranged. The default is a vertical <see cref="LinearItemsLayout"/>.</value>
		/// <remarks>
		/// Use <see cref="LinearItemsLayout"/> for single-row or single-column layouts, or <see cref="GridItemsLayout"/> for multi-column grids.
		/// The layout determines the scrolling direction and how items are positioned relative to each other.
		/// </remarks>
		public IItemsLayout ItemsLayout
		{
			get => InternalItemsLayout;
			set => InternalItemsLayout = value;
		}

		/// <summary>Bindable property for <see cref="ItemSizingStrategy"/>.</summary>
		public static readonly BindableProperty ItemSizingStrategyProperty =
			BindableProperty.Create(nameof(ItemSizingStrategy), typeof(ItemSizingStrategy), typeof(ItemsView));

		/// <summary>
		/// Gets or sets the strategy used to measure and size items in the view.
		/// </summary>
		/// <value>An <see cref="ItemSizingStrategy"/> value that determines how items are measured.</value>
		/// <remarks>
		/// The sizing strategy affects performance and layout behavior.
		/// Use <see cref="ItemSizingStrategy.MeasureAllItems"/> to measure each item individually for accurate sizing,
		/// or <see cref="ItemSizingStrategy.MeasureFirstItem"/> to use the first item's size as a template for better performance with uniform items.
		/// </remarks>
		public ItemSizingStrategy ItemSizingStrategy
		{
			get => (ItemSizingStrategy)GetValue(ItemSizingStrategyProperty);
			set => SetValue(ItemSizingStrategyProperty, value);
		}
	}
}