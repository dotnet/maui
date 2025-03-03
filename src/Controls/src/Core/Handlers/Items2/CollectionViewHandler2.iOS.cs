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

		public CollectionViewHandler2() : base(Mapper)
		{


		}
		public CollectionViewHandler2(PropertyMapper mapper = null) : base(mapper ?? Mapper)
		{

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
		};
	}

	public partial class CollectionViewHandler2 : ItemsViewHandler2<ReorderableItemsView>
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
			if (WillNeedScrollAdjustment(args))
			{
				if (args.IsAnimated)
				{
					(Controller as GroupableItemsViewController2<ReorderableItemsView>)?.SetScrollAnimationEndedCallback(() => base.ScrollToRequested(sender, args));
				}
				else
				{
					base.ScrollToRequested(sender, args);
				}
			}

			base.ScrollToRequested(sender, args);
		}

		public static void MapIsGrouped(CollectionViewHandler2 handler, GroupableItemsView itemsView)
		{
			handler.Controller?.UpdateItemsSource();
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

			SubscribeToItemsLayoutPropertyChanged(itemsLayout);

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
			//	(handler.Controller as StructuredItemsViewController2<ReorderableItemsView>)?.UpdateHeaderView();
		}

		public static void MapFooterTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
			//(handler.Controller as StructuredItemsViewController2<ReorderableItemsView>)?.UpdateFooterView();
		}

		public static void MapItemsLayout(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		public static void MapItemSizingStrategy(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		void SubscribeToItemsLayoutPropertyChanged(IItemsLayout itemsLayout)
		{
			if (itemsLayout is not null)
			{
				itemsLayout.PropertyChanged += (sender, args) =>
				{
					if (args.PropertyName == nameof(ItemsLayout.SnapPointsAlignment) ||
						args.PropertyName == nameof(ItemsLayout.SnapPointsType) ||
						args.PropertyName == nameof(GridItemsLayout.VerticalItemSpacing) ||
						args.PropertyName == nameof(GridItemsLayout.HorizontalItemSpacing) ||
						args.PropertyName == nameof(GridItemsLayout.Span) ||
						args.PropertyName == nameof(LinearItemsLayout.ItemSpacing))

					{
						UpdateLayout();
					}
				};
			}
		}
	}
}
