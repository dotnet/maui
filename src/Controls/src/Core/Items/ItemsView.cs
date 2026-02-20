#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.View"/> that serves as a base class for views that contain a templated list of items.</summary>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public abstract class ItemsView : View
	{

		/// <summary>Bindable property for <see cref="EmptyView"/>.</summary>
		public static readonly BindableProperty EmptyViewProperty =
			BindableProperty.Create(nameof(EmptyView), typeof(object), typeof(ItemsView), null);

		/// <summary>
		/// Gets or sets the view or object to display when the <see cref="ItemsSource"/> is empty or <see langword="null"/>.
		/// </summary>
		/// <value>The empty view content, which can be a string, view, or any object. Use <see cref="EmptyViewTemplate"/> to customize rendering.</value>
		/// <remarks>
		/// The empty view provides user feedback when there are no items to display.
		/// If <see cref="EmptyViewTemplate"/> is set, it is used to render the empty view; otherwise, the object's string representation or the view itself is displayed.
		/// </remarks>
		public object EmptyView
		{
			get => GetValue(EmptyViewProperty);
			set => SetValue(EmptyViewProperty, value);
		}

		/// <summary>Bindable property for <see cref="EmptyViewTemplate"/>.</summary>
		public static readonly BindableProperty EmptyViewTemplateProperty =
			BindableProperty.Create(nameof(EmptyViewTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		/// <summary>
		/// Gets or sets the <see cref="DataTemplate"/> used to render the <see cref="EmptyView"/>.
		/// </summary>
		/// <value>A <see cref="DataTemplate"/> that defines how the empty view is displayed, or <see langword="null"/> to use default rendering.</value>
		/// <remarks>
		/// The template's binding context is set to the <see cref="EmptyView"/> object.
		/// This allows for rich, custom layouts to be displayed when the collection is empty.
		/// </remarks>
		public DataTemplate EmptyViewTemplate
		{
			get => (DataTemplate)GetValue(EmptyViewTemplateProperty);
			set => SetValue(EmptyViewTemplateProperty, value);
		}

		/// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsView), null);

		/// <summary>
		/// Gets or sets the collection of items to be displayed.
		/// </summary>
		/// <value>An <see cref="IEnumerable"/> collection of items to display, or <see langword="null"/> to display the <see cref="EmptyView"/>.</value>
		/// <remarks>
		/// The items source can be any collection that implements <see cref="IEnumerable"/>.
		/// For automatic UI updates when the collection changes, use an <see cref="ObservableCollection{T}"/> or implement <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>.
		/// </remarks>
		public IEnumerable ItemsSource
		{
			get => (IEnumerable)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		/// <summary>Bindable property for <see cref="RemainingItemsThresholdReachedCommand"/>.</summary>
		public static readonly BindableProperty RemainingItemsThresholdReachedCommandProperty =
			BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommand), typeof(ICommand), typeof(ItemsView), null);

		/// <summary>
		/// Gets or sets the command to execute when the remaining items threshold is reached during scrolling.
		/// </summary>
		/// <value>An <see cref="ICommand"/> to execute, or <see langword="null"/> for no command.</value>
		/// <remarks>
		/// This command is commonly used to implement incremental loading (infinite scroll).
		/// When scrolling reaches the threshold specified by <see cref="RemainingItemsThreshold"/>, this command is executed,
		/// allowing you to load more items asynchronously.
		/// </remarks>
		public ICommand RemainingItemsThresholdReachedCommand
		{
			get => (ICommand)GetValue(RemainingItemsThresholdReachedCommandProperty);
			set => SetValue(RemainingItemsThresholdReachedCommandProperty, value);
		}

		/// <summary>Bindable property for <see cref="RemainingItemsThresholdReachedCommandParameter"/>.</summary>
		public static readonly BindableProperty RemainingItemsThresholdReachedCommandParameterProperty = BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommandParameter), typeof(object), typeof(ItemsView), default(object));

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="RemainingItemsThresholdReachedCommand"/>.
		/// </summary>
		/// <value>The command parameter, or <see langword="null"/> if no parameter is needed.</value>
		public object RemainingItemsThresholdReachedCommandParameter
		{
			get => GetValue(RemainingItemsThresholdReachedCommandParameterProperty);
			set => SetValue(RemainingItemsThresholdReachedCommandParameterProperty, value);
		}

		/// <summary>Bindable property for <see cref="HorizontalScrollBarVisibility"/>.</summary>
		public static readonly BindableProperty HorizontalScrollBarVisibilityProperty = BindableProperty.Create(
			nameof(HorizontalScrollBarVisibility),
			typeof(ScrollBarVisibility),
			typeof(ItemsView),
			ScrollBarVisibility.Default);

		/// <summary>
		/// Gets or sets the visibility of the horizontal scroll bar.
		/// </summary>
		/// <value>A <see cref="ScrollBarVisibility"/> value. The default is <see cref="ScrollBarVisibility.Default"/>.</value>
		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
			set => SetValue(HorizontalScrollBarVisibilityProperty, value);
		}

		/// <summary>Bindable property for <see cref="VerticalScrollBarVisibility"/>.</summary>
		public static readonly BindableProperty VerticalScrollBarVisibilityProperty = BindableProperty.Create(
			nameof(VerticalScrollBarVisibility),
			typeof(ScrollBarVisibility),
			typeof(ItemsView),
			ScrollBarVisibility.Default);

		/// <summary>
		/// Gets or sets the visibility of the vertical scroll bar.
		/// </summary>
		/// <value>A <see cref="ScrollBarVisibility"/> value. The default is <see cref="ScrollBarVisibility.Default"/>.</value>
		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
			set => SetValue(VerticalScrollBarVisibilityProperty, value);
		}

		/// <summary>Bindable property for <see cref="RemainingItemsThreshold"/>.</summary>
		public static readonly BindableProperty RemainingItemsThresholdProperty =
			BindableProperty.Create(nameof(RemainingItemsThreshold), typeof(int), typeof(ItemsView), -1, validateValue: (bindable, value) => (int)value >= -1);

		/// <summary>
		/// Gets or sets the number of remaining items in the view that trigger the <see cref="RemainingItemsThresholdReached"/> event.
		/// </summary>
		/// <value>The threshold count. Must be -1 or greater. The default is -1 (disabled).</value>
		/// <remarks>
		/// When scrolling reaches a point where only this many items remain to be displayed, the <see cref="RemainingItemsThresholdReached"/> event fires.
		/// Set to -1 to disable the threshold behavior. This is commonly used for implementing infinite scroll or incremental loading.
		/// </remarks>
		public int RemainingItemsThreshold
		{
			get => (int)GetValue(RemainingItemsThresholdProperty);
			set => SetValue(RemainingItemsThresholdProperty, value);
		}

		internal static readonly BindableProperty InternalItemsLayoutProperty =
			BindableProperty.Create(nameof(ItemsLayout), typeof(IItemsLayout), typeof(ItemsView),
				null, propertyChanged: OnInternalItemsLayoutPropertyChanged,
				defaultValueCreator: (b) => LinearItemsLayout.CreateVerticalDefault());

		static void OnInternalItemsLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is BindableObject boOld)
				SetInheritedBindingContext(boOld, null);

			if (newValue is BindableObject boNew)
				SetInheritedBindingContext(boNew, bindable.BindingContext);
		}

		protected IItemsLayout InternalItemsLayout
		{
			get => (IItemsLayout)GetValue(InternalItemsLayoutProperty);
			set => SetValue(InternalItemsLayoutProperty, value);
		}

		/// <summary>Bindable property for <see cref="ItemTemplate"/>.</summary>
		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemsView));

		/// <summary>
		/// Gets or sets the <see cref="DataTemplate"/> used to render each item in the collection.
		/// </summary>
		/// <value>A <see cref="DataTemplate"/> that defines how items are displayed, or <see langword="null"/> to use default rendering.</value>
		/// <remarks>
		/// The template's binding context is set to each item in the <see cref="ItemsSource"/>.
		/// For dynamic template selection based on item data, use a <see cref="DataTemplateSelector"/> instead.
		/// </remarks>
		public DataTemplate ItemTemplate
		{
			get => (DataTemplate)GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		/// <summary>Bindable property for <see cref="ItemsUpdatingScrollMode"/>.</summary>
		public static readonly BindableProperty ItemsUpdatingScrollModeProperty =
			BindableProperty.Create(nameof(ItemsUpdatingScrollMode), typeof(ItemsUpdatingScrollMode), typeof(ItemsView),
				default(ItemsUpdatingScrollMode));

		/// <summary>
		/// Gets or sets the scroll behavior when items are added, removed, or updated in the collection.
		/// </summary>
		/// <value>An <see cref="ItemsUpdatingScrollMode"/> value. The default is <see cref="ItemsUpdatingScrollMode.KeepItemsInView"/>.</value>
		/// <remarks>
		/// This property controls how the view maintains its scroll position when the <see cref="ItemsSource"/> changes.
		/// Use <see cref="ItemsUpdatingScrollMode.KeepLastItemInView"/> for chat-like interfaces where new items are added at the end.
		/// </remarks>
		public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
		{
			get => (ItemsUpdatingScrollMode)GetValue(ItemsUpdatingScrollModeProperty);
			set => SetValue(ItemsUpdatingScrollModeProperty, value);
		}

		/// <summary>
		/// Scrolls the items view to the item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to scroll to.</param>
		/// <param name="groupIndex">The zero-based index of the group containing the item, or -1 if not grouped.</param>
		/// <param name="position">The position where the item should appear in the visible area after scrolling.</param>
		/// <param name="animate"><see langword="true"/> to animate the scroll; <see langword="false"/> to jump immediately.</param>
		public void ScrollTo(int index, int groupIndex = -1,
			ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			if (DismissScroll())
				return;

			OnScrollToRequested(new ScrollToRequestEventArgs(index, groupIndex, position, animate));
		}

		/// <summary>
		/// Scrolls the items view to the specified data item.
		/// </summary>
		/// <param name="item">The data item to scroll to.</param>
		/// <param name="group">The group object containing the item, or <see langword="null"/> if not grouped.</param>
		/// <param name="position">The position where the item should appear in the visible area after scrolling.</param>
		/// <param name="animate"><see langword="true"/> to animate the scroll; <see langword="false"/> to jump immediately.</param>
		public void ScrollTo(object item, object group = null,
			ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			if (DismissScroll())
				return;

			OnScrollToRequested(new ScrollToRequestEventArgs(item, group, position, animate));
		}

		/// <summary>
		/// Manually triggers the remaining items threshold behavior.
		/// </summary>
		/// <remarks>
		/// This method raises the <see cref="RemainingItemsThresholdReached"/> event and executes the <see cref="RemainingItemsThresholdReachedCommand"/>.
		/// It's typically called by platform-specific renderers when scrolling reaches the threshold, but can be called manually if needed.
		/// </remarks>
		public void SendRemainingItemsThresholdReached()
		{
			RemainingItemsThresholdReached?.Invoke(this, EventArgs.Empty);

			if (RemainingItemsThresholdReachedCommand?.CanExecute(RemainingItemsThresholdReachedCommandParameter) == true)
				RemainingItemsThresholdReachedCommand?.Execute(RemainingItemsThresholdReachedCommandParameter);

			OnRemainingItemsThresholdReached();
		}

		/// <param name="e">The event arguments.</param>
		public void SendScrolled(ItemsViewScrolledEventArgs e)
		{
			Scrolled?.Invoke(this, e);

			OnScrolled(e);
		}

		public event EventHandler<ScrollToRequestEventArgs> ScrollToRequested;

		public event EventHandler<ItemsViewScrolledEventArgs> Scrolled;

		public event EventHandler RemainingItemsThresholdReached;

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			// TODO hartez 2018-05-22 05:04 PM This 40,40 is what LV1 does; can we come up with something less arbitrary?
			var minimumSize = new Size(40, 40);

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			var maxWidth = Math.Min(scaled.Width, widthConstraint);
			var maxHeight = Math.Min(scaled.Height, heightConstraint);

			Size request = new Size(maxWidth, maxHeight);

			return new SizeRequest(request, minimumSize);
		}

		protected virtual void OnScrollToRequested(ScrollToRequestEventArgs e)
		{
			ScrollToRequested?.Invoke(this, e);
		}

		protected virtual void OnRemainingItemsThresholdReached()
		{

		}

		protected virtual void OnScrolled(ItemsViewScrolledEventArgs e)
		{

		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			if (InternalItemsLayout is BindableObject bo)
				SetInheritedBindingContext(bo, BindingContext);
		}

		private protected override string GetDebuggerDisplay()
		{
			var itemsSourceText = DebuggerDisplayHelpers.GetDebugText(nameof(ItemsSource), ItemsSource?.GetType());
			return $"{base.GetDebuggerDisplay()}, {itemsSourceText}";
		}

		private bool DismissScroll()
		{
			return ItemsSource is null || (ItemsSource is IEnumerable<object> items && !items.Any());
		}
	}
}
