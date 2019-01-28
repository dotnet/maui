using System;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class SelectableItemsViewController : ItemsViewController
	{
		protected readonly SelectableItemsView SelectableItemsView;

		public SelectableItemsViewController(SelectableItemsView selectableItemsView, ItemsViewLayout layout) 
			: base(selectableItemsView, layout)
		{
			SelectableItemsView = selectableItemsView;
			Delegator.SelectableItemsViewController = this;
		}

		// _Only_ called if the user initiates the selection change; will not be called for programmatic selection
		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			UpdateFormsSelection();
		}

		// _Only_ called if the user initiates the selection change; will not be called for programmatic selection
		public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			UpdateFormsSelection();
		}

		internal void ClearSelection()
		{
			var paths = CollectionView.GetIndexPathsForSelectedItems();

			foreach (var path in paths)
			{
				CollectionView.DeselectItem(path, false);
			}
		}

		// Called by Forms to mark an item selected 
		internal void SelectItem(object selectedItem)
		{
			var index = GetIndexForItem(selectedItem);
			CollectionView.SelectItem(index, true, UICollectionViewScrollPosition.None);
		}

		void UpdateFormsSelection()
		{
			var mode = SelectableItemsView.SelectionMode;

			switch (mode)
			{
				case SelectionMode.None:
					SelectableItemsView.SelectedItem = null;
					// TODO hartez Clear SelectedItems
					return;
				case SelectionMode.Single:
					var paths = CollectionView.GetIndexPathsForSelectedItems();
					if (paths.Length > 0)
					{
						SelectableItemsView.SelectedItem = GetItemAtIndex(paths[0]);
					}
					// TODO hartez Clear SelectedItems
					return;
				case SelectionMode.Multiple:
					// TODO hartez Handle setting SelectedItems to all the items at the selected paths	
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal void UpdateSelectionMode()
		{
			var mode = SelectableItemsView.SelectionMode;

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

			UpdateFormsSelection();
		}
	}
}