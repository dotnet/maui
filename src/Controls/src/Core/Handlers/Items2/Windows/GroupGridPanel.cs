using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WStackPanel = Microsoft.UI.Xaml.Controls.StackPanel;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// A native StackPanel that represents a single group in grouped grid mode.
	/// Contains: GroupHeader + Grid (with items arranged in columns) + GroupFooter.
	/// <para>
	/// Uses a WinUI <see cref="WGrid"/> with star-sized columns and ColumnSpacing/RowSpacing
	/// for the item grid. This approach uses fully native layout — no custom MeasureOverride
	/// or ArrangeOverride needed.
	/// </para>
	/// </summary>
	internal class GroupGridPanel : WStackPanel
	{
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
		/// Native elements for each item in the grid. Indexed to match <see cref="ItemDataObjects"/>.
		/// </summary>
		internal List<FrameworkElement> ItemElements { get; } = new();

		internal WGrid? ItemGrid { get; private set; }

		/// <summary>
		/// Builds the group panel with header, item grid, and footer.
		/// </summary>
		/// <param name="context">The group context containing data and templates.</param>
		/// <param name="itemsView">The parent CollectionView for logical child management.</param>
		/// <param name="span">Number of columns in the grid.</param>
		/// <param name="horizontalSpacing">Horizontal spacing between grid items.</param>
		/// <param name="verticalSpacing">Vertical spacing between grid items.</param>
		internal void Build(
			GroupGridContext context,
			ItemsView itemsView,
			int span,
			double horizontalSpacing,
			double verticalSpacing)
		{
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

			// Item Grid
			ItemGrid = CreateItemGrid(context, itemsView, mauiContext, span, horizontalSpacing, verticalSpacing);
			Children.Add(ItemGrid);

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
		}

		WGrid CreateItemGrid(
			GroupGridContext context,
			ItemsView itemsView,
			IMauiContext mauiContext,
			int span,
			double horizontalSpacing,
			double verticalSpacing)
		{
			var grid = new WGrid
			{
				ColumnSpacing = horizontalSpacing,
				RowSpacing = verticalSpacing,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};

			// Create column definitions for N columns
			for (int i = 0; i < span; i++)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			}

			if (context.Items is null || context.ItemTemplate is null)
				return grid;

			int cellIndex = 0;
			foreach (var item in context.Items)
			{
				if (item is null)
					continue;

				int row = cellIndex / span;
				int col = cellIndex % span;

				// Ensure enough row definitions
				while (grid.RowDefinitions.Count <= row)
				{
					grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				}

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

					WGrid.SetRow(nativeElement, row);
					WGrid.SetColumn(nativeElement, col);
					grid.Children.Add(nativeElement);

					ItemDataObjects.Add(item);
					ItemElements.Add(nativeElement);

					// Add tap handler for selection support
					nativeElement.Tapped += OnItemTapped;
				}

				cellIndex++;
			}

			return grid;
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

			// Walk up to find the CollectionViewHandler2 and notify it of the item tap
			// The handler will update the MAUI CollectionView's selection
			if (FindItemsView() is CollectionView collectionView)
			{
				HandleItemSelection(collectionView, item);
			}
		}

		ItemsView? FindItemsView()
		{
			// Walk up the MAUI view hierarchy from any of our managed views
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
					collectionView.SelectedItem = collectionView.SelectedItem == item ? null : item;
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
			for (int i = 0; i < ItemDataObjects.Count && i < MauiViews.Count; i++)
			{
				// MauiViews includes header/footer, so we need to offset.
				// ItemDataObjects only has grid items, so the MAUI views for items
				// start after the header (if present).
				// Find the corresponding MAUI view by matching via the native element.
				if (i < ItemElements.Count)
				{
					var nativeElement = ItemElements[i];
					var mauiView = FindMauiViewForNative(nativeElement);
					if (mauiView is VisualElement ve)
					{
						var data = ItemDataObjects[i];
						bool isSelected = itemsView.SelectedItem == data ||
							itemsView.SelectedItems.Contains(data);
						VisualStateManager.GoToState(ve,
							isSelected ? VisualStateManager.CommonStates.Selected : VisualStateManager.CommonStates.Normal);
					}
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
			ItemGrid = null;
		}
	}
}
