#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Graphics;
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

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var width = widthConstraint;
			var height = heightConstraint;

			if (!double.IsInfinity(width))
				width = Context.ToPixels(width);

			if (!double.IsInfinity(height))
				height = Context.ToPixels(height);

			UpdateEmptyViewSize(width, height);

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void PlatformArrange(Rect frame)
		{
			var width = Context.ToPixels(frame.Width);
			var height = Context.ToPixels(frame.Height);

			UpdateEmptyViewSize(width, height);

			base.PlatformArrange(frame);
		}

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

		void UpdateEmptyViewSize(double width, double height)
		{
			var adapter = PlatformView.GetAdapter();

			if (adapter is EmptyViewAdapter emptyViewAdapter)
			{
				var emptyView = emptyViewAdapter.EmptyView ?? emptyViewAdapter.EmptyViewTemplate;
				Size size = Size.Zero;

				IView view = emptyView as IView ?? (emptyView as DataTemplate)?.CreateContent() as IView;

				if (view is not null)
				{
					if (view.Handler is null)
					{
						TemplateHelpers.GetHandler(view as View, this.MauiContext);
					}

					size = view.Measure(double.PositiveInfinity, double.PositiveInfinity);
				}

				var measuredHeight = !double.IsInfinity(size.Height) ? Context.ToPixels(size.Height) : height;
				emptyViewAdapter.RecyclerViewWidth = width;
				emptyViewAdapter.RecyclerViewHeight = measuredHeight > 0 ? measuredHeight : height;
			}
		}
	}
}