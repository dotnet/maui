using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class SelectableItemsViewController<TItemsView> : StructuredItemsViewController<TItemsView>
		where TItemsView : SelectableItemsView
	{
		public SelectableItemsViewController(TItemsView selectableItemsView, ItemsViewLayout layout)
			: base(selectableItemsView, layout)
		{
		}

		protected override UICollectionViewDelegateFlowLayout CreateDelegator()
		{
			return new SelectableItemsViewDelegator<TItemsView, SelectableItemsViewController<TItemsView>>(ItemsViewLayout, this);
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
				CollectionView.SelectItem(index, true, UICollectionViewScrollPosition.None);
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

		internal void UpdateNativeSelection()
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
					SynchronizeNativeSelectionWithSelectedItems();
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
					break;
				case SelectionMode.Single:
					CollectionView.AllowsSelection = true;
					CollectionView.AllowsMultipleSelection = false;
					break;
				case SelectionMode.Multiple:
					CollectionView.AllowsSelection = true;
					CollectionView.AllowsMultipleSelection = true;
					break;
			}

			UpdateNativeSelection();
		}

		void SynchronizeNativeSelectionWithSelectedItems()
		{
			var selectedItems = ItemsView.SelectedItems;
			var selectedIndexPaths = CollectionView.GetIndexPathsForSelectedItems();

			foreach (var path in selectedIndexPaths)
			{
				var itemAtPath = GetItemAtIndex(path);
				if (ShouldNotBeSelected(itemAtPath, selectedItems))
				{
					CollectionView.DeselectItem(path, true);
				}
			}

			foreach (var item in selectedItems)
			{
				SelectItem(item);
			}
		}

		bool ShouldNotBeSelected(object item, IList<object> selectedItems)
		{
			for (int n = 0; n < selectedItems.Count; n++)
			{
				if (selectedItems[n] == item)
				{
					return false;
				}
			}

			return true;
		}
	}
}