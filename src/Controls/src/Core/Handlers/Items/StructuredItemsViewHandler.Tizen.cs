#nullable disable
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override TCollectionView CreatePlatformView()
		{
			return new MauiStructuredItemsView<TItemsView>();
		}

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateAdaptor();
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateAdaptor();
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateLayoutManager();
		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateLayoutManager();
		}

		public static void MapFooter(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateAdaptor();
		}
		public static void MapHeader(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateAdaptor();
		}

		//TODO Make this public in .NET10
		internal static void MapItemsLayoutPropertyChanged(StructuredItemsViewHandler<TItemsView> handler, TItemsView view, object arg3)
		{
			(handler.PlatformView as MauiCollectionView<TItemsView>)?.UpdateLayoutManager();
		}
	}
}
