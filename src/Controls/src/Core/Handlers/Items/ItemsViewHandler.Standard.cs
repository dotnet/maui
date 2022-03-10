using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract partial class ItemsViewHandler<TItemsView> : ViewHandler<TItemsView, object> where TItemsView : ItemsView
	{
		protected override object CreatePlatformView()
		{
			throw new NotImplementedException();
		}

		public static void MapItemsSource(IItemsViewHandler handler, ItemsView itemsView)
		{
		}

		public static void MapHorizontalScrollBarVisibility(IItemsViewHandler handler, ItemsView itemsView)
		{
		}

		public static void MapVerticalScrollBarVisibility(IItemsViewHandler handler, ItemsView itemsView)
		{
		}

		public static void MapItemTemplate(IItemsViewHandler handler, ItemsView itemsView)
		{
		}

		public static void MapEmptyView(IItemsViewHandler handler, ItemsView itemsView)
		{
		}

		public static void MapEmptyViewTemplate(IItemsViewHandler handler, ItemsView itemsView)
		{
		}

		public static void MapFlowDirection(IItemsViewHandler handler, ItemsView itemsView)
		{
		}

		public static void MapIsVisible(IItemsViewHandler handler, ItemsView itemsView)
		{
		}

		public static void MapItemsUpdatingScrollMode(IItemsViewHandler handler, ItemsView itemsView)
		{
		}
	}
}
