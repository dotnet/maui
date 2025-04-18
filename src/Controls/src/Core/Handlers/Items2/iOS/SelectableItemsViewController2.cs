#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class SelectableItemsViewController2<TItemsView> : StructuredItemsViewController2<TItemsView>
		where TItemsView : SelectableItemsView
	{
		public SelectableItemsViewController2(TItemsView selectableItemsView, UICollectionViewLayout layout)
			: base(selectableItemsView, layout)
		{
		}

		protected override UICollectionViewDelegateFlowLayout CreateDelegator()
		{
			return new SelectableItemsViewDelegator2<TItemsView, SelectableItemsViewController2<TItemsView>>(ItemsViewLayout, this);
		}

		// _Only_ called if the user initiates the selection change; will not be called for programmatic selection
		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			FormsSelectItem(indexPath);
		}

		// _Only_ called if the user initiates the selection change; will not be called for programmatic selection
		public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			FormsDeselectItem(indexPath);
		}

		// Called by Forms to mark an item selected 
		internal void SelectItem(object selectedItem)
		{
			var index = GetIndexForItem(selectedItem);

			if (index.Section > -1 && index.Item > -1)
			{
				// Ensure the selected index is updated after the collection view's items generation is completed
				CollectionView.PerformBatchUpdates(null, _ =>
				{
					CollectionView.SelectItem(index, true, UICollectionViewScrollPosition.None);
				});
			}
		}

		// Called by Forms to clear the native selection
		internal void ClearSelection()
		{
			var selectedItemIndexes = CollectionView.GetIndexPathsForSelectedItems();

			foreach (var index in selectedItemIndexes)
			{
				CollectionView.DeselectItem(index, true);
			}
		}

		void FormsSelectItem(NSIndexPath indexPath)
		{
			var mode = ItemsView.SelectionMode;

			switch (mode)
			{
				case SelectionMode.None:
					break;
				case SelectionMode.Single:
					ItemsView.SelectedItem = GetItemAtIndex(indexPath);
					break;
				case SelectionMode.Multiple:
					ItemsView.SelectedItems.Add(GetItemAtIndex(indexPath));
					break;
			}
		}

		void FormsDeselectItem(NSIndexPath indexPath)
		{
			var mode = ItemsView.SelectionMode;

			switch (mode)
			{
				case SelectionMode.None:
					break;
				case SelectionMode.Single:
					break;
				case SelectionMode.Multiple:
					ItemsView.SelectedItems.Remove(GetItemAtIndex(indexPath));
					break;
			}
		}

		internal void UpdatePlatformSelection()
		{
			if (ItemsView == null)
			{
				return;
			}

			var mode = ItemsView.SelectionMode;

			switch (mode)
			{
				case SelectionMode.None:
					return;
				case SelectionMode.Single:
					var selectedItem = ItemsView.SelectedItem;

					if (selectedItem != null)
					{
						SelectItem(selectedItem);
					}
					else
					{
						// SelectedItem has been set to null; if an item is selected, we need to de-select it
						ClearSelection();
					}

					return;
				case SelectionMode.Multiple:
					SynchronizePlatformSelectionWithSelectedItems();
					break;
			}
		}

		internal void UpdateSelectionMode()
		{
			var mode = ItemsView.SelectionMode;

			switch (mode)
			{
				case SelectionMode.None:
					CollectionView.AllowsSelection = false;
					CollectionView.AllowsMultipleSelection = false;
					ClearsSelectionOnViewWillAppear = true;
					break;
				case SelectionMode.Single:
					CollectionView.AllowsSelection = true;
					CollectionView.AllowsMultipleSelection = false;
					ClearsSelectionOnViewWillAppear = false;
					break;
				case SelectionMode.Multiple:
					CollectionView.AllowsSelection = true;
					CollectionView.AllowsMultipleSelection = true;
					ClearsSelectionOnViewWillAppear = false;
					break;
			}

			UpdatePlatformSelection();
			CollectionView.UpdateAccessibilityTraits(ItemsView);
		}

		void SynchronizePlatformSelectionWithSelectedItems()
		{
			var selectedItems = ItemsView.SelectedItems.ToHashSet();
			var selectedIndexPaths = CollectionView.GetIndexPathsForSelectedItems();

			foreach (var path in selectedIndexPaths)
			{
				var itemAtPath = GetItemAtIndex(path);
				if (!selectedItems.Contains(itemAtPath))
				{
					CollectionView.DeselectItem(path, true);
				}
				else
				{
					selectedItems.Remove(itemAtPath);
				}
			}

			foreach (var item in selectedItems)
			{
				SelectItem(item);
			}
		}
	}
}
