using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class CarouselViewOnScrollListener : RecyclerViewScrollListener<CarouselView, IItemsViewSource>
	{
		readonly CarouselView _carouselView;
		readonly CarouselViewLoopManager _carouselViewLoopManager;

		public CarouselViewOnScrollListener(ItemsView itemsView, ItemsViewAdapter<CarouselView, IItemsViewSource> itemsViewAdapter, CarouselViewLoopManager carouselViewLoopManager) : base((CarouselView)itemsView, itemsViewAdapter, true)
		{
			_carouselView = itemsView as CarouselView;
			_carouselViewLoopManager = carouselViewLoopManager;
		}

		public override void OnScrollStateChanged(RecyclerView recyclerView, int state)
		{
			base.OnScrollStateChanged(recyclerView, state);

			if (_carouselView.IsSwipeEnabled)
			{
				if (state == RecyclerView.ScrollStateDragging)
					_carouselView.SetIsDragging(true);
				else
					_carouselView.SetIsDragging(false);
			}

			_carouselView.IsScrolling = state != RecyclerView.ScrollStateIdle;
		}

		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			base.OnScrolled(recyclerView, dx, dy);

			if (_carouselView.Loop)
			{
				//We could have a race condition where we are scrolling our collection to center the first item
				//We save that ScrollToEventARgs and call it again
				_carouselViewLoopManager.CheckPendingScrollToEvents(recyclerView);
			}
		}

		protected override (int First, int Center, int Last) GetVisibleItemsIndex(RecyclerView recyclerView)
		{
			var firstVisibleItemIndex = -1;
			var lastVisibleItemIndex = -1;
			var centerItemIndex = -1;

			if (recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager)
			{
				var firstView = recyclerView.FindViewHolderForAdapterPosition(linearLayoutManager.FindFirstVisibleItemPosition());
				var lastView = recyclerView.FindViewHolderForAdapterPosition(linearLayoutManager.FindLastVisibleItemPosition());
				var centerView = recyclerView.GetCenteredView();
				firstVisibleItemIndex = GetIndexFromTemplatedCell(firstView?.ItemView);
				lastVisibleItemIndex = GetIndexFromTemplatedCell(lastView?.ItemView);
				centerItemIndex = GetIndexFromTemplatedCell(centerView);
			}

			return (firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}

		int GetIndexFromTemplatedCell(global::Android.Views.View view)
		{
			int itemIndex = -1;

			if (view is ItemContentView templatedCell)
			{
				var bContext = (templatedCell?.View as VisualElement)?.BindingContext;
				itemIndex = ItemsViewAdapter.GetPositionForItem(bContext);
			}

			return itemIndex;
		}
	}
}
