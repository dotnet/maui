#nullable disable
using System.Collections;

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

		/// <summary>Bindable property for <see cref="AnchorMode"/>.</summary>
		public static readonly BindableProperty AnchorModeProperty =
			BindableProperty.Create(nameof(AnchorMode), typeof(ScrollAnchorMode), typeof(StructuredItemsView), ScrollAnchorMode.None,
				propertyChanged: OnAnchorModeChanged);

		/// <summary>
		/// Gets or sets which edges of the content stay in view while the content changes. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="ScrollAnchorMode"/> value. The default is <see cref="ScrollAnchorMode.None"/>.</value>
		/// <remarks>
		/// <para>
		/// With <see cref="ScrollAnchorMode.End"/>, the view shows the end of its content when it first appears, and keeps it
		/// in view while items are added or removed, any item (including the last one) grows or shrinks, the header or footer
		/// changes, or the view resizes. Following never animates. When the user scrolls away from the end, the view stops
		/// following it and <see cref="IsAnchored"/> becomes <see langword="false"/>; it follows again once the end is back in
		/// view, whether the user scrolled there or the app did, with <see cref="ScrollToEnd(bool)"/> or
		/// <see cref="ItemsView.ScrollTo(int, int, ScrollToPosition, bool)"/>.
		/// </para>
		/// <para>
		/// <see cref="ScrollAnchorMode.Start"/> does the same for the start of the content, above the header: items added
		/// before the first item while the view is at its start come into view instead of being added above it. The view
		/// follows its start again once the user scrolls back to it, or the app calls <see cref="ScrollToStart(bool)"/>.
		/// </para>
		/// <para>
		/// With both, the view follows whichever edge it is at, and starts at the end. When it is at both because its content
		/// fits, it follows the end. Changing the anchor mode starts over, as when the view first appears.
		/// </para>
		/// <para>
		/// While the view follows an edge, <see cref="ItemsView.ItemsUpdatingScrollMode"/> doesn't scroll it; while it
		/// doesn't, <see cref="ItemsView.ItemsUpdatingScrollMode"/> applies as usual.
		/// </para>
		/// <para>
		/// Anchoring applies to vertical layouts on Android, iOS, and Mac Catalyst. Elsewhere it has no effect, and
		/// <see cref="IsAnchored"/> stays <see langword="false"/>.
		/// </para>
		/// </remarks>
		public ScrollAnchorMode AnchorMode
		{
			get => (ScrollAnchorMode)GetValue(AnchorModeProperty);
			set => SetValue(AnchorModeProperty, value);
		}

		static readonly BindablePropertyKey IsAnchoredPropertyKey =
			BindableProperty.CreateReadOnly(nameof(IsAnchored), typeof(bool), typeof(StructuredItemsView), false);

		/// <summary>Bindable property for <see cref="IsAnchored"/>.</summary>
		public static readonly BindableProperty IsAnchoredProperty = IsAnchoredPropertyKey.BindableProperty;

		/// <summary>
		/// Gets whether the view follows an edge of its content that <see cref="AnchorMode"/> selects. This is a bindable property.
		/// </summary>
		/// <value>
		/// <see langword="true"/> while the view follows the start or the end of its content; <see langword="false"/> after the
		/// user scrolls away from it, and wherever the view can't follow it (before it's displayed, in a horizontal layout,
		/// or on a platform that doesn't support anchoring).
		/// </value>
		/// <remarks>
		/// Use it, for example, to show a "jump to latest" button that calls <see cref="ScrollToEnd(bool)"/> while the user
		/// reads earlier items.
		/// </remarks>
		public bool IsAnchored => (bool)GetValue(IsAnchoredProperty);

		internal void SetIsAnchored(bool value) => SetValue(IsAnchoredPropertyKey, value);

		/// <summary>Scrolls to the start of the content: before the first item, to the start of the header.</summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <remarks>
		/// <para>
		/// With <see cref="ScrollAnchorMode.Start"/>, the view follows its start again. Unlike scrolling to the first item with
		/// <see cref="ItemsView.ScrollTo(int, int, ScrollToPosition, bool)"/>, it reaches the start when there's a header.
		/// </para>
		/// <para>
		/// It scrolls to the start of vertical layouts on Android, iOS, and Mac Catalyst. Elsewhere, it scrolls to the first item.
		/// </para>
		/// </remarks>
		public void ScrollToStart(bool animate = true)
		{
			if (Handler is IScrollToEdgeHandler handler && handler.ScrollToStart(animate))
			{
				return;
			}

			ScrollToFirstItem(animate);
		}

		/// <summary>Scrolls to the end of the content: past the last item, to the end of the footer.</summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <remarks>
		/// <para>
		/// With <see cref="ScrollAnchorMode.End"/>, the view follows its end again. Unlike scrolling to the last item with
		/// <see cref="ItemsView.ScrollTo(int, int, ScrollToPosition, bool)"/>, it reaches the end when there's a footer.
		/// </para>
		/// <para>
		/// It scrolls to the end of vertical layouts on Android, iOS, and Mac Catalyst. Elsewhere, it scrolls to the last item.
		/// </para>
		/// </remarks>
		public void ScrollToEnd(bool animate = true)
		{
			if (Handler is IScrollToEdgeHandler handler && handler.ScrollToEnd(animate))
			{
				return;
			}

			ScrollToLastItem(animate);
		}

		// Where the platform can't scroll to the start of the content, the first item is as close as ScrollTo gets.
		internal virtual void ScrollToFirstItem(bool animate)
		{
			if (CountItems(ItemsSource) > 0)
			{
				ScrollTo(0, position: ScrollToPosition.Start, animate: animate);
			}
		}

		// Where the platform can't scroll to the end of the content, the last item is as close as ScrollTo gets.
		internal virtual void ScrollToLastItem(bool animate)
		{
			var count = CountItems(ItemsSource);
			if (count > 0)
			{
				ScrollTo(count - 1, position: ScrollToPosition.End, animate: animate);
			}
		}

		internal static int CountItems(IEnumerable items)
		{
			if (items is ICollection collection)
			{
				return collection.Count;
			}

			var count = 0;
			if (items is not null)
			{
				foreach (var _ in items)
				{
					count++;
				}
			}

			return count;
		}

		static void OnAnchorModeChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// Only a platform view that anchors reports being anchored; with anchoring off, the view never is.
			if (((ScrollAnchorMode)newValue & (ScrollAnchorMode.Start | ScrollAnchorMode.End)) == 0)
			{
				((StructuredItemsView)bindable).SetIsAnchored(false);
			}
		}
	}
}