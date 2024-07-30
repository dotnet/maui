using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
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
			var groupInfo = new LayoutGroupingInfo();

			if (ItemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
			{
				groupInfo.IsGrouped = groupableItemsView.IsGrouped;
				groupInfo.HasHeader = groupableItemsView.GroupHeaderTemplate is not null;
				groupInfo.HasFooter = groupableItemsView.GroupFooterTemplate is not null;
			}

			var itemSizingStrategy = ItemsView.ItemSizingStrategy;
			var itemsLayout = ItemsView.ItemsLayout;

			//TODO: Find a better way to do this 
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

			if (itemsLayout is GridItemsLayout gridItemsLayout)
			{
				return LayoutFactory2.CreateGrid(gridItemsLayout, groupInfo);
			}

			if (itemsLayout is LinearItemsLayout listItemsLayout)
			{
				return LayoutFactory2.CreateList(listItemsLayout, groupInfo);
			}

			// Fall back to vertical list
			return LayoutFactory2.CreateList(new LinearItemsLayout(ItemsLayoutOrientation.Vertical), groupInfo);
		}

		public static void MapHeaderTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			(handler.Controller as StructuredItemsViewController2<ReorderableItemsView>)?.UpdateHeaderView();
		}

		public static void MapFooterTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			(handler.Controller as StructuredItemsViewController2<ReorderableItemsView>)?.UpdateFooterView();
		}

		public static void MapItemsLayout(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		public static void MapItemSizingStrategy(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}
	}
}
