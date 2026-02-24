using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WStackPanel = Microsoft.UI.Xaml.Controls.StackPanel;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// A native StackPanel that represents a single group in grouped grid mode.
	/// Contains: GroupHeader + VariableSizedWrapGrid (with grid items) + GroupFooter.
	/// <para>
	/// Uses WinUI <see cref="VariableSizedWrapGrid"/> with <see cref="VariableSizedWrapGrid.MaximumRowsOrColumns"/>
	/// set to the grid span. ItemWidth is dynamically calculated from the available width
	/// via <see cref="SizeChanged"/> so columns divide the space equally.
	/// </para>
	/// <para>
	/// This mirrors how grouped linear mode works with StackLayout — each group is a single item
	/// in the outer ItemsView, and the native panel handles item arrangement within the group.
	/// </para>
	/// </summary>
	internal class GroupGridPanel : WStackPanel
	{
		VariableSizedWrapGrid? _wrapGrid;
		int _span = 1;
		double _horizontalSpacing;

		/// <summary>
		/// All MAUI views created for this group (header, items, footer).
		/// Must be removed from the CollectionView's logical children when recycled.
		/// </summary>
		internal List<View> MauiViews { get; } = new();

		/// <summary>
		/// Item data objects in this group, in order. Used for selection hit-testing.
		/// </summary>
		internal List<object> ItemDataObjects { get; } = new();

		/// <summary>
		/// Native elements for each item in the wrap grid. Indexed to match <see cref="ItemDataObjects"/>.
		/// </summary>
		internal List<FrameworkElement> ItemElements { get; } = new();

		internal VariableSizedWrapGrid? WrapGrid => _wrapGrid;

		/// <summary>
		/// Builds the group panel with header, VariableSizedWrapGrid, and footer.
		/// </summary>
		internal void Build(
			GroupGridContext context,
			ItemsView itemsView,
			int span,
			double horizontalSpacing,
			double verticalSpacing)
		{
			_span = span;
			_horizontalSpacing = horizontalSpacing;

			Orientation = Orientation.Vertical;
			HorizontalAlignment = HorizontalAlignment.Stretch;

			var mauiContext = itemsView.Handler?.MauiContext;
			if (mauiContext is null)
				return;

			// Group Header
			if (context.GroupHeaderTemplate is not null)
			{
				var headerElement = CreateTemplatedElement(
					context.GroupHeaderTemplate, context.Group, itemsView, mauiContext);
				if (headerElement is not null)
				{
					Children.Add(headerElement);
				}
			}

			// Item Grid using VariableSizedWrapGrid
			_wrapGrid = CreateWrapGrid(context, itemsView, mauiContext, span, horizontalSpacing, verticalSpacing);
			Children.Add(_wrapGrid);

			// Group Footer
			if (context.GroupFooterTemplate is not null)
			{
				var footerElement = CreateTemplatedElement(
					context.GroupFooterTemplate, context.Group, itemsView, mauiContext);
				if (footerElement is not null)
				{
					Children.Add(footerElement);
				}
			}

			// Listen for size changes to recalculate ItemWidth
			SizeChanged += OnPanelSizeChanged;
		}

		VariableSizedWrapGrid CreateWrapGrid(
			GroupGridContext context,
			ItemsView itemsView,
			IMauiContext mauiContext,
			int span,
			double horizontalSpacing,
			double verticalSpacing)
		{
			var wrapGrid = new VariableSizedWrapGrid
			{
				Orientation = Orientation.Horizontal,
				MaximumRowsOrColumns = span,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};

			if (context.Items is null || context.ItemTemplate is null)
				return wrapGrid;

			foreach (var item in context.Items)
			{
				if (item is null)
					continue;

				var template = context.ItemTemplate;
				if (template is DataTemplateSelector selector)
				{
					template = selector.SelectTemplate(item, itemsView);
				}

				var viewContent = template?.CreateContent() as View;
				if (viewContent is not null)
				{
					viewContent.BindingContext = item;
					itemsView.AddLogicalChild(viewContent);
					MauiViews.Add(viewContent);

					var nativeElement = viewContent.ToPlatform(mauiContext);
					nativeElement.HorizontalAlignment = HorizontalAlignment.Stretch;
					nativeElement.VerticalAlignment = VerticalAlignment.Stretch;

					// Apply spacing via margin — VariableSizedWrapGrid doesn't have ColumnSpacing/RowSpacing
					nativeElement.Margin = new Thickness(
						horizontalSpacing / 2, verticalSpacing / 2,
						horizontalSpacing / 2, verticalSpacing / 2);

					wrapGrid.Children.Add(nativeElement);

					ItemDataObjects.Add(item);
					ItemElements.Add(nativeElement);

					// Add tap handler for selection support
					nativeElement.Tapped += OnItemTapped;
				}
			}

			return wrapGrid;
		}

		/// <summary>
		/// Recalculates ItemWidth when the panel's width changes, so items divide
		/// the available space equally based on span.
		/// </summary>
		void OnPanelSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_wrapGrid is null || e.NewSize.Width <= 0)
				return;

			var availableWidth = e.NewSize.Width;

			// Account for margins: each item has horizontalSpacing/2 on each side
			// Total margin per item = horizontalSpacing; total for all items in a row = span * horizontalSpacing
			var totalMargin = _span * _horizontalSpacing;
			var itemWidth = (availableWidth - totalMargin) / _span;

			if (itemWidth > 0)
			{
				_wrapGrid.ItemWidth = itemWidth;
			}
		}

		FrameworkElement? CreateTemplatedElement(
			DataTemplate template,
			object bindingContext,
			ItemsView itemsView,
			IMauiContext mauiContext)
		{
			var resolvedTemplate = template;
			if (template is DataTemplateSelector selector)
			{
				resolvedTemplate = selector.SelectTemplate(bindingContext, itemsView);
			}

			var viewContent = resolvedTemplate?.CreateContent() as View;
			if (viewContent is null)
				return null;

			viewContent.BindingContext = bindingContext;
			itemsView.AddLogicalChild(viewContent);
			MauiViews.Add(viewContent);

			var nativeElement = viewContent.ToPlatform(mauiContext);
			nativeElement.HorizontalAlignment = HorizontalAlignment.Stretch;

			return nativeElement;
		}

		void OnItemTapped(object sender, TappedRoutedEventArgs e)
		{
			if (sender is not FrameworkElement element)
				return;

			int itemIndex = ItemElements.IndexOf(element);
			if (itemIndex < 0 || itemIndex >= ItemDataObjects.Count)
				return;

			var item = ItemDataObjects[itemIndex];

			if (FindItemsView() is CollectionView collectionView)
			{
				HandleItemSelection(collectionView, item);
			}
		}

		ItemsView? FindItemsView()
		{
			foreach (var view in MauiViews)
			{
				if (view.Parent is ItemsView iv)
					return iv;
			}
			return null;
		}

		static void HandleItemSelection(CollectionView collectionView, object item)
		{
			switch (collectionView.SelectionMode)
			{
				case SelectionMode.Single:
					collectionView.SelectedItem = object.Equals(collectionView.SelectedItem, item) ? null : item;
					break;

				case SelectionMode.Multiple:
					var selectedItems = new List<object>(collectionView.SelectedItems);
					if (selectedItems.Contains(item))
					{
						selectedItems.Remove(item);
					}
					else
					{
						selectedItems.Add(item);
					}
					collectionView.UpdateSelectedItems(selectedItems);
					break;

				case SelectionMode.None:
				default:
					break;
			}
		}

		/// <summary>
		/// Updates visual states for all items based on current selection.
		/// </summary>
		internal void UpdateVisualStates(SelectableItemsView itemsView)
		{
			for (int i = 0; i < ItemDataObjects.Count && i < ItemElements.Count; i++)
			{
				var nativeElement = ItemElements[i];
				var mauiView = FindMauiViewForNative(nativeElement);
				if (mauiView is VisualElement ve)
				{
					var data = ItemDataObjects[i];
					bool isSelected = object.Equals(itemsView.SelectedItem, data) ||
						itemsView.SelectedItems.Contains(data);
					VisualStateManager.GoToState(ve,
						isSelected ? VisualStateManager.CommonStates.Selected : VisualStateManager.CommonStates.Normal);
				}
			}
		}

		View? FindMauiViewForNative(FrameworkElement nativeElement)
		{
			foreach (var view in MauiViews)
			{
				if (view.Handler?.PlatformView == nativeElement)
					return view;
			}
			return null;
		}

		/// <summary>
		/// Removes all MAUI views from the ItemsView's logical children and detaches event handlers.
		/// Must be called when the group panel is recycled.
		/// </summary>
		internal void CleanUp(ItemsView itemsView)
		{
			SizeChanged -= OnPanelSizeChanged;

			// Detach tap handlers
			foreach (var element in ItemElements)
			{
				element.Tapped -= OnItemTapped;
			}

			// Remove logical children
			foreach (var mauiView in MauiViews)
			{
				itemsView.RemoveLogicalChild(mauiView);
			}

			MauiViews.Clear();
			ItemDataObjects.Clear();
			ItemElements.Clear();
			_wrapGrid = null;
		}
	}
}
