#nullable disable
using System.Collections.Generic;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class CarouselViewLoopManager
	{
		public const int LoopScale = 16384;

		IItemsViewSource _itemsSource;
		readonly Queue<ScrollToRequestEventArgs> _pendingScrollTo = new Queue<ScrollToRequestEventArgs>();

		public void CenterIfNeeded(RecyclerView recyclerView, bool isHorizontal)
		{
			if (!(recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager))
				return;
			if (_itemsSource is null)
				return;

			var itemSourceCount = _itemsSource.Count;

			var firstCompletelyItemVisible = linearLayoutManager.FindFirstCompletelyVisibleItemPosition();

			var offSet = recyclerView.ComputeHorizontalScrollOffset();

			if (!isHorizontal)
				offSet = recyclerView.ComputeVerticalScrollOffset();

			if (firstCompletelyItemVisible == 0)
				linearLayoutManager.ScrollToPositionWithOffset(itemSourceCount, -offSet);
		}

		public void CheckPendingScrollToEvents(RecyclerView recyclerView)
		{
			if (recyclerView is not IMauiRecyclerView<ItemsView> mauiRecyclerView)
				return;

			if (_pendingScrollTo.TryDequeue(out ScrollToRequestEventArgs scrollToRequestEventArgs))
				mauiRecyclerView.ScrollTo(scrollToRequestEventArgs);
		}

		public void AddPendingScrollTo(ScrollToRequestEventArgs args) => _pendingScrollTo.Enqueue(args);

		public int GetGoToIndex(RecyclerView recyclerView, int carouselPosition, int newPosition)
		{
			if (!(recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager))
				return -1;

			if (_itemsSource is null || _itemsSource.Count == 0)
				return -1;

			var currentCarouselPosition = carouselPosition;
			var itemSourceCount = _itemsSource.Count;

			var diffToStart = (currentCarouselPosition - newPosition + itemSourceCount) % itemSourceCount;
			var diffToEnd = (newPosition - currentCarouselPosition + itemSourceCount) % itemSourceCount;

			var centerView = recyclerView.GetCenteredView();

			if (centerView == null)
				return -1;

			var centerPosition = linearLayoutManager.GetPosition(centerView);
			var increment = currentCarouselPosition - newPosition;
			var incrementAbs = System.Math.Abs(increment);

			int goToPosition;
			if (diffToStart < incrementAbs)
				goToPosition = centerPosition - diffToStart;
			else if (diffToEnd < incrementAbs)
				goToPosition = centerPosition + diffToEnd;
			else
				goToPosition = centerPosition - increment;

			return goToPosition;
		}

		public void SetItemsSource(IItemsViewSource itemsSource) => _itemsSource = itemsSource;
	}
}
