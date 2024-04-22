#nullable disable
using Android.Graphics;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Platform
{
	public static class RecyclerViewExtensions
	{
		public static void UpdateSelection(this RecyclerView recyclerView, SelectableItemsView selectableItemsView)
		{
			var mode = selectableItemsView.SelectionMode;
			//TODO: on NET8 implement a ISelectableItemsViewAdapter interface on the adapter
			var adapter = recyclerView.GetAdapter() as ReorderableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>;
			if (adapter == null)
				return;

			adapter.ClearPlatformSelection();

			switch (mode)
			{
				case SelectionMode.None:
					return;

				case SelectionMode.Single:
					var selectedItem = selectableItemsView.SelectedItem;
					adapter.MarkPlatformSelection(selectedItem);
					return;

				case SelectionMode.Multiple:
					var selectedItems = selectableItemsView.SelectedItems;
					foreach (var item in selectedItems)
					{
						adapter.MarkPlatformSelection(item);
					}
					return;
			}
		}

		internal static double GetMaxHeight(this RecyclerView recyclerView, ItemsView itemsView, double heightConstraint)
		{
			var remainingItemsThreshold = itemsView.RemainingItemsThreshold;

			if (remainingItemsThreshold == -1)
				return heightConstraint;

			double visibleItemsHeight = 0;

			LinearLayoutManager layoutManager = recyclerView.GetLayoutManager() as LinearLayoutManager;

			if (layoutManager != null)
			{
				var firstVisibleIndex = layoutManager.FindFirstCompletelyVisibleItemPosition();

				if (firstVisibleIndex == -1)
					return heightConstraint;

				var lastVisibleIndex = layoutManager.FindLastCompletelyVisibleItemPosition();

				for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
				{
					var cell = layoutManager.FindViewByPosition(i);
					Rect rect = new Rect();
					bool isVisible = cell.GetLocalVisibleRect(rect);

					if (isVisible)
						visibleItemsHeight += cell.Height;
				}
			}

			return visibleItemsHeight;
		}
	}
}