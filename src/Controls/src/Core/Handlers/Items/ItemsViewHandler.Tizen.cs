using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using TCollectionView = Tizen.UIExtensions.ElmSharp.CollectionView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract partial class ItemsViewHandler<TItemsView> : ViewHandler<TItemsView, TCollectionView> where TItemsView : ItemsView
	{
		protected ItemsViewHandler(PropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
		{
		}

		protected override TCollectionView CreatePlatformView()
		{
			return new TCollectionView(NativeParent);
		}

		public static void MapItemsSource(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateItemsSource(itemsView);
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.HorizontalScrollBarVisiblePolicy = itemsView.HorizontalScrollBarVisibility.ToPlatform();
		}

		public static void MapVerticalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.VerticalScrollBarVisiblePolicy = itemsView.VerticalScrollBarVisibility.ToPlatform();
		}

		public static void MapItemTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateAdaptor(itemsView);
		}

		public static void MapEmptyView(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateAdaptor(itemsView);
		}

		public static void MapEmptyViewTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateAdaptor(itemsView);
		}

		public static void MapFlowDirection(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}
		public static void MapIsVisible(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateVisibility(itemsView);
		}
		public static void MapItemsUpdatingScrollMode(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}
	}
}
