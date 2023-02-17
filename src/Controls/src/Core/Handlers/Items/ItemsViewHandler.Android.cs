#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract partial class ItemsViewHandler<TItemsView> : ViewHandler<TItemsView, RecyclerView> where TItemsView : ItemsView
	{
		protected ItemsViewHandler(PropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
		{
		}
		protected abstract IItemsLayout GetItemsLayout();

		protected virtual ItemsViewAdapter<TItemsView, IItemsViewSource> CreateAdapter() => new(VirtualView);

		protected override void ConnectHandler(RecyclerView platformView)
		{
			base.ConnectHandler(platformView);
			(platformView as IMauiRecyclerView<TItemsView>)?.SetUpNewElement(VirtualView);
		}

		protected override void DisconnectHandler(RecyclerView platformView)
		{
			base.DisconnectHandler(platformView);
			(platformView as IMauiRecyclerView<TItemsView>)?.TearDownOldElement(VirtualView);
		}

		protected override RecyclerView CreatePlatformView() =>
			new MauiRecyclerView<TItemsView, ItemsViewAdapter<TItemsView, IItemsViewSource>, IItemsViewSource>(Context, GetItemsLayout, CreateAdapter);

		public static void MapItemsSource(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateItemsSource();
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateHorizontalScrollBarVisibility();
		}

		public static void MapVerticalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateHorizontalScrollBarVisibility();
		}

		public static void MapItemTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateItemTemplate();
		}

		public static void MapEmptyView(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateEmptyView();
		}

		public static void MapEmptyViewTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateEmptyView();
		}

		public static void MapFlowDirection(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateFlowDirection();
		}

		public static void MapIsVisible(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapItemsUpdatingScrollMode(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateScrollingMode();
		}
	}
}
