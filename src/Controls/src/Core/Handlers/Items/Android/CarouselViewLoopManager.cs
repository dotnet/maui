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

			var itemSourceCount = _itemsSource.Count;

			if (newPosition < 0 || newPosition >= itemSourceCount)
			{
				return -1;
			}

			var centerView = recyclerView.GetCenteredView();

			if (centerView == null)
				return -1;

			var centerPosition = linearLayoutManager.GetPosition(centerView);
			var adapterCount = recyclerView.GetAdapter()?.ItemCount ?? 0;

			return GetNearestAdapterPosition(centerPosition, newPosition, itemSourceCount, adapterCount);
		}

		public void SetItemsSource(IItemsViewSource itemsSource) => _itemsSource = itemsSource;

		static int GetNearestAdapterPosition(int currentAdapterPosition, int targetItemIndex, int itemCount, int adapterCount)
		{
			if (currentAdapterPosition < 0 || adapterCount <= 0 || itemCount <= 0)
			{
				return -1;
			}

			var currentCycleStart = currentAdapterPosition - (currentAdapterPosition % itemCount);
			var bestPosition = -1;
			var bestDistance = int.MaxValue;

			for (var cycleOffset = -1; cycleOffset <= 1; cycleOffset++)
			{
				var candidate = currentCycleStart + targetItemIndex + (cycleOffset * itemCount);

				if (candidate < 0 || candidate >= adapterCount)
				{
					continue;
				}

				var distance = System.Math.Abs(candidate - currentAdapterPosition);
				if (distance < bestDistance)
				{
					bestPosition = candidate;
					bestDistance = distance;
				}
			}

			return bestPosition;
		}
	}
}
