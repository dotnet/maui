#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class LayoutHeaderFooterInfo
	{
		public IView FooterView { get; set; }
		public IView HeaderView { get; set; }
		public DataTemplate FooterTemplate { get; set; }
		public DataTemplate HeaderTemplate { get; set; }
		public bool HasHeader { get; set; }
		public bool HasFooter { get; set; }
	}

	internal class LayoutGroupingInfo
	{
		public bool IsGrouped { get; set; }
		public bool HasHeader { get; set; }
		public bool HasFooter { get; set; }
	}

	internal class LayoutSnapInfo
	{
		public SnapPointsAlignment SnapAligment { get; set; }
		public SnapPointsType SnapType { get; set; }
	}

	public partial class CollectionViewHandler2
	{
		// Cache for MeasureFirstItem optimization
		CoreGraphics.CGSize _firstItemMeasuredSize = CoreGraphics.CGSize.Empty;

		public CollectionViewHandler2() : base(Mapper)
		{


		}
		public CollectionViewHandler2(PropertyMapper mapper = null) : base(mapper ?? Mapper)
		{

		}

		/// <summary>
		/// Gets the cached first item measured size for MeasureFirstItem optimization.
		/// Returns CGSize.Empty if not cached or not using MeasureFirstItem strategy.
		/// </summary>
		internal CoreGraphics.CGSize GetCachedFirstItemSize()
		{
			if (VirtualView is CollectionView cv && cv.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
			{
				return _firstItemMeasuredSize;
			}
			return CoreGraphics.CGSize.Empty;
		}

		/// <summary>
		/// Sets the cached first item measured size for MeasureFirstItem optimization.
		/// </summary>
		internal void SetCachedFirstItemSize(CoreGraphics.CGSize size)
		{
			if (VirtualView is CollectionView cv && cv.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
			{
				_firstItemMeasuredSize = size;
			}
		}

		public static PropertyMapper<CollectionView, CollectionViewHandler2> Mapper = new(ItemsViewMapper)
		{
			[ReorderableItemsView.CanReorderItemsProperty.PropertyName] = MapCanReorderItems,
			[GroupableItemsView.IsGroupedProperty.PropertyName] = MapIsGrouped,
			[SelectableItemsView.SelectedItemProperty.PropertyName] = MapSelectedItem,
			[SelectableItemsView.SelectedItemsProperty.PropertyName] = MapSelectedItems,
			[SelectableItemsView.SelectionModeProperty.PropertyName] = MapSelectionMode,
			[StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeaderTemplate,
			[StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooterTemplate,
			[StructuredItemsView.HeaderProperty.PropertyName] = MapHeaderTemplate,
			[StructuredItemsView.FooterProperty.PropertyName] = MapFooterTemplate,
			[StructuredItemsView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
			[StructuredItemsView.ItemSizingStrategyProperty.PropertyName] = MapItemSizingStrategy,
			[GroupableItemsView.GroupHeaderTemplateProperty.PropertyName] = MapHeaderTemplate,
			[GroupableItemsView.GroupFooterTemplateProperty.PropertyName] = MapFooterTemplate,
			[StructuredItemsView.AnchorModeProperty.PropertyName] = MapAnchorMode,
		};
	}

	public partial class CollectionViewHandler2 : ItemsViewHandler2<ReorderableItemsView>, IScrollToEdgeHandler
	{
		// Reorderable
		protected override ItemsViewController2<ReorderableItemsView> CreateController(ReorderableItemsView itemsView, UICollectionViewLayout layout)
			 => new ReorderableItemsViewController2<ReorderableItemsView>(itemsView, layout);

		public static void MapCanReorderItems(CollectionViewHandler2 handler, ReorderableItemsView itemsView)
		{
			(handler.Controller as ReorderableItemsViewController2<ReorderableItemsView>)?.UpdateCanReorderItems();
		}

		// Groupable
		protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			// Scrolling to an item measures the rows it brings into view, which can move the item: a grouped list that measures
			// all its items scrolls to it again once they're measured. Not when the app made a newer request from the Scrolled
			// event the first scroll raised, nor for an item the view doesn't have: that scrolls nowhere, and leaves what an
			// earlier request left for later.
			if (WillNeedScrollAdjustment(args) && !args.IsAnimated && !TryScrollTo(args))
			{
				return;
			}

			if (TryScrollTo(args, out var animating) && WillNeedScrollAdjustment(args) && args.IsAnimated)
			{
				// An animated scroll scrolls to the item again once UIKit reports that its animation ended. UIKit doesn't animate a
				// view that isn't in a window, nor to an item that's where the request puts it already: it scrolls the view at
				// once and reports the end before the scroll returns, or never reports one. Scroll again now then, rather than
				// when a later animation ends. Before iOS and Mac Catalyst 17, which don't tell, an animation runs in a window.
				var animationEnds = OperatingSystem.IsIOSVersionAtLeast(17) || OperatingSystem.IsMacCatalystVersionAtLeast(17)
					? animating
					: Controller.CollectionView.Window is not null;
				if (animationEnds)
				{
					(Controller as GroupableItemsViewController2<ReorderableItemsView>)?.SetScrollAnimationEndedCallback(() => base.ScrollToRequested(sender, args));
				}
				else
				{
					base.ScrollToRequested(sender, args);
				}
			}
		}

		public static void MapIsGrouped(CollectionViewHandler2 handler, GroupableItemsView itemsView)
		{
			handler.Controller?.UpdateItemsSource();
			handler.UpdateLayout();
		}

		bool WillNeedScrollAdjustment(ScrollToRequestEventArgs args)
		{
			return ItemsView.ItemSizingStrategy == ItemSizingStrategy.MeasureAllItems
				&& ItemsView.IsGrouped
				&& (args.ScrollToPosition == ScrollToPosition.End || args.ScrollToPosition == ScrollToPosition.MakeVisible);
		}


		// Selectable
		public static void MapItemsSource(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
			ItemsViewHandler2<ReorderableItemsView>.MapItemsSource(handler, itemsView);
			MapSelectedItem(handler, itemsView);
		}

		public static void MapSelectedItem(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController2<ReorderableItemsView>)?.UpdatePlatformSelection();
		}

		public static void MapSelectedItems(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController2<ReorderableItemsView>)?.UpdatePlatformSelection();
		}

		public static void MapSelectionMode(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController2<ReorderableItemsView>)?.UpdateSelectionMode();
		}


		// Structured
		protected override UICollectionViewLayout SelectLayout()
		{
			var headerFooterInfo = new LayoutHeaderFooterInfo();
			var groupInfo = new LayoutGroupingInfo();

			if (ItemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
			{
				groupInfo.IsGrouped = groupableItemsView.IsGrouped;
				groupInfo.HasHeader = groupableItemsView.GroupHeaderTemplate is not null;
				groupInfo.HasFooter = groupableItemsView.GroupFooterTemplate is not null;
			}

			if (ItemsView is StructuredItemsView structuredItemsView)
			{
				headerFooterInfo.HeaderTemplate = structuredItemsView.HeaderTemplate;
				headerFooterInfo.FooterTemplate = structuredItemsView.FooterTemplate;
				if (structuredItemsView.Header is View headerView)
				{
					headerFooterInfo.HeaderView = headerView;
				}
				if (structuredItemsView.Footer is View footerView)
				{
					headerFooterInfo.FooterView = footerView;
				}
				headerFooterInfo.HasHeader = structuredItemsView.Header is not null || structuredItemsView.HeaderTemplate is not null;
				headerFooterInfo.HasFooter = structuredItemsView.Footer is not null || structuredItemsView.FooterTemplate is not null;
			}

			var itemSizingStrategy = ItemsView.ItemSizingStrategy;
			var itemsLayout = ItemsView.ItemsLayout;

			if (itemsLayout is GridItemsLayout gridItemsLayout)
			{
				return LayoutFactory2.CreateGrid(gridItemsLayout, groupInfo, headerFooterInfo);
			}

			if (itemsLayout is LinearItemsLayout listItemsLayout)
			{
				return LayoutFactory2.CreateList(listItemsLayout, groupInfo, headerFooterInfo);
			}

			// Fall back to vertical list
			var fallbackItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
			// Manually setting the value to ensure the property changed event is properly wired..
			ItemsView.ItemsLayout = fallbackItemsLayout;
			return LayoutFactory2.CreateList(fallbackItemsLayout, groupInfo, headerFooterInfo);
		}

		public static void MapHeaderTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
			(handler.Controller as StructuredItemsViewController2<ReorderableItemsView>)?.UpdateHeaderView();
		}

		public static void MapFooterTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
			(handler.Controller as StructuredItemsViewController2<ReorderableItemsView>)?.UpdateFooterView();
		}

		public static void MapItemsLayout(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateItemsLayoutSubscription(itemsView.ItemsLayout);
			handler.UpdateLayout();
		}

		public static void MapItemSizingStrategy(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		/// <summary>Maps <see cref="StructuredItemsView.AnchorMode"/> to the platform view.</summary>
		/// <param name="handler">The handler for the collection view.</param>
		/// <param name="itemsView">The collection view.</param>
		public static void MapAnchorMode(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.Controller?.UpdateAnchorMode();
		}

		bool IScrollToEdgeHandler.ScrollToStart(bool animate) =>
			Controller is not null && (Controller.ScrollToStart(animate) || ScrollToEdgeItem(end: false, animate));

		bool IScrollToEdgeHandler.ScrollToEnd(bool animate) =>
			Controller is not null && (Controller.ScrollToEnd(animate) || ScrollToEdgeItem(end: true, animate));

		// Where the view doesn't scroll to an edge of its content, as in a horizontal layout, the first or last item is as close as
		// it gets: the first or last the view shows, which an items source that can be read only once doesn't give again.
		bool ScrollToEdgeItem(bool end, bool animate)
		{
			var collectionView = Controller.CollectionView;
			var sections = (int)collectionView.NumberOfSections();
			for (var index = 0; index < sections; index++)
			{
				var section = end ? sections - 1 - index : index;
				var count = (int)collectionView.NumberOfItemsInSection(section);
				if (count > 0)
				{
					var position = end ? ScrollToPosition.End : ScrollToPosition.Start;
					ScrollToRequested(this, new ScrollToRequestEventArgs(end ? count - 1 : 0, ItemsView.IsGrouped ? section : -1, position, animate));
					break;
				}
			}

			return true;
		}

		Dictionary<string, object> _layoutPropertyCache = new();
		IItemsLayout _subscribedItemsLayout;

		protected override void DisconnectHandler(UIView platformView)
		{
			base.DisconnectHandler(platformView);
			_layoutPropertyCache?.Clear();
			_layoutPropertyCache = null;
			UpdateItemsLayoutSubscription(null);
		}

		internal void UpdateItemsLayoutSubscription(IItemsLayout newLayout)
		{
			if (_subscribedItemsLayout == newLayout)
			{
				return;
			}

			if (_subscribedItemsLayout is not null)
			{
				_subscribedItemsLayout.PropertyChanged -= OnItemsLayoutPropertyChanged;
			}

			_subscribedItemsLayout = newLayout;

			if (_subscribedItemsLayout is not null)
			{
				// Reinitialize the cache if it was cleared by DisconnectHandler
				_layoutPropertyCache ??= new Dictionary<string, object>();
				_subscribedItemsLayout.PropertyChanged += OnItemsLayoutPropertyChanged;
			}
		}

		void OnItemsLayoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
		{
			if (_subscribedItemsLayout == null)
			{
				return;
			}

			// Handle all layout-affecting property changes with caching
			if (args.PropertyName == nameof(ItemsLayout.SnapPointsAlignment) ||
				args.PropertyName == nameof(ItemsLayout.SnapPointsType) ||
				args.PropertyName == nameof(GridItemsLayout.VerticalItemSpacing) ||
				args.PropertyName == nameof(GridItemsLayout.HorizontalItemSpacing) ||
				args.PropertyName == nameof(GridItemsLayout.Span) ||
				args.PropertyName == nameof(LinearItemsLayout.ItemSpacing))
			{
				// Get the current value of the changed property
				object newValue = GetPropertyValue(_subscribedItemsLayout, args.PropertyName);

				// Check if value actually changed by comparing with cached value
				if (!_layoutPropertyCache.TryGetValue(args.PropertyName, out var cachedValue) ||
					!Equals(cachedValue, newValue))
				{
					// Update cache and trigger layout update
					_layoutPropertyCache[args.PropertyName] = newValue;
					UpdateLayout();
				}
			}
		}

		object GetPropertyValue(IItemsLayout itemsLayout, string propertyName)
		{
			return propertyName switch
			{
				nameof(GridItemsLayout.Span) when itemsLayout is GridItemsLayout grid => grid.Span,
				nameof(GridItemsLayout.HorizontalItemSpacing) when itemsLayout is GridItemsLayout grid => grid.HorizontalItemSpacing,
				nameof(GridItemsLayout.VerticalItemSpacing) when itemsLayout is GridItemsLayout grid => grid.VerticalItemSpacing,
				nameof(LinearItemsLayout.ItemSpacing) when itemsLayout is LinearItemsLayout linear => linear.ItemSpacing,
				nameof(ItemsLayout.SnapPointsAlignment) when itemsLayout is GridItemsLayout grid => grid.SnapPointsAlignment,
				nameof(ItemsLayout.SnapPointsAlignment) when itemsLayout is LinearItemsLayout linear => linear.SnapPointsAlignment,
				nameof(ItemsLayout.SnapPointsType) when itemsLayout is GridItemsLayout grid => grid.SnapPointsType,
				nameof(ItemsLayout.SnapPointsType) when itemsLayout is LinearItemsLayout linear => linear.SnapPointsType,
				_ => null
			};
		}
	}
}
