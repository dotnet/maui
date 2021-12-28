using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract partial class ItemsViewHandler<TItemsView> : ViewHandler<TItemsView, Tizen.UIExtensions.NUI.CollectionView> where TItemsView : ItemsView
	{
		protected ItemsViewHandler(PropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
		{
		}

		protected override Tizen.UIExtensions.NUI.CollectionView CreatePlatformView()
		{
			throw new NotImplementedException();
		}

		public static void MapItemsSource(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapVerticalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapItemTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapEmptyView(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapEmptyViewTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapFlowDirection(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}
		public static void MapIsVisible(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}
		public static void MapItemsUpdatingScrollMode(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}
	}
}
