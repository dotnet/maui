#nullable disable
#pragma warning disable RS0016 // Add public types and members to the declared API

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class CollectionViewHandler2 : ItemsViewHandler2<ReorderableItemsView>
	{
		protected override IItemsLayout Layout { get => ItemsView?.ItemsLayout; }

		public static void MapCanReorderItems(CollectionViewHandler2 handler, ReorderableItemsView itemsView)
		{
		}

		public static void MapIsGrouped(CollectionViewHandler2 handler, GroupableItemsView itemsView)
		{
			
		}

		public static void MapItemsSource(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
			ItemsViewHandler2<ReorderableItemsView>.MapItemsSource(handler, itemsView);
		}

		public static void MapSelectedItem(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
			
		}

		public static void MapSelectedItems(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectionMode(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
		}

		public static void MapHeaderTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
		}

		public static void MapFooterTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
		}

		public static void MapItemsLayout(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
		}

		public static void MapItemSizingStrategy(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
		}
	}
}

#pragma warning restore RS0016 // Add public types and members to the declared API