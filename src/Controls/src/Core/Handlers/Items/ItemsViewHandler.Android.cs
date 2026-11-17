#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;

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
			if (VirtualView is not IView view)
				return base.GetDesiredSize(widthConstraint, heightConstraint);

			// Match the native constraints, including explicit sizes and minimum/maximum requests.
			var widthSpec = Context.CreateMeasureSpec(widthConstraint, view.Width, view.MinimumWidth, view.MaximumWidth);
			var heightSpec = Context.CreateMeasureSpec(heightConstraint, view.Height, view.MinimumHeight, view.MaximumHeight);
			var width = AView.MeasureSpec.GetMode(widthSpec) == MeasureSpecMode.Unspecified
				? double.PositiveInfinity : AView.MeasureSpec.GetSize(widthSpec);
			var height = AView.MeasureSpec.GetMode(heightSpec) == MeasureSpecMode.Unspecified
				? double.PositiveInfinity : AView.MeasureSpec.GetSize(heightSpec);

			UpdateEmptyViewSize(width, height);

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void PlatformArrange(Rect frame)
		{
			var width = Context.ToPixels(frame.Width);
			var height = Context.ToPixels(frame.Height);

			UpdateEmptyViewSize(width, height);

			base.PlatformArrange(frame);

			if (PlatformView?.GetAdapter() is EmptyViewAdapter emptyViewAdapter)
			{
				var itemView = GetEmptyViewHolder(PlatformView)?.ItemView;
				if (itemView is not null && !emptyViewAdapter.HasValidEmptyViewSize(itemView, PlatformView))
				{
					// Auto-measure can finish child layout before the final arrange size is known.
					// Only request another pass if that size was not applied, deferring during layout.
					itemView.ForceLayout();
					PlatformInterop.RequestLayoutIfNeeded(itemView);
				}
			}
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
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateVerticalScrollBarVisibility();
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
			var platformView = PlatformView;
			var adapter = platformView.GetAdapter();

			if (adapter is EmptyViewAdapter emptyViewAdapter)
			{
				double tolerance = 0.001;
				var widthChanged = Math.Abs(emptyViewAdapter.RecyclerViewWidth - width) > tolerance;
				var heightChanged = Math.Abs(emptyViewAdapter.RecyclerViewHeight - height) > tolerance;

				emptyViewAdapter.RecyclerViewWidth = width;
				emptyViewAdapter.RecyclerViewHeight = height;

				if (widthChanged || heightChanged)
				{
					GetEmptyViewHolder(platformView)?.ItemView.ForceLayout();
				}
			}
		}

		RecyclerView.ViewHolder GetEmptyViewHolder(RecyclerView platformView)
		{
			var structuredItemsView = VirtualView as StructuredItemsView;
			var hasHeader = (structuredItemsView?.Header ?? structuredItemsView?.HeaderTemplate) is not null;
			return platformView.FindViewHolderForAdapterPosition(hasHeader ? 1 : 0);
		}
	}
}