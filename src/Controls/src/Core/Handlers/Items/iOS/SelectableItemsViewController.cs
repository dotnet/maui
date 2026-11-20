#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	[Obsolete("This type is obsolete on iOS and Mac Catalyst. Use Microsoft.Maui.Controls.Handlers.Items2.SelectableItemsViewController2<TItemsView> instead.")]
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
			if (ItemsView?.ItemsSource is null || !ItemsView.IsEnabled)
			{
				return;
			}

			var tap = CreateItemTappedEventArgs(indexPath);
			FormsSelectItem(indexPath);
			QueueItemTapped(tap, selected: true);
		}

		// _Only_ called if the user initiates the selection change; will not be called for programmatic selection
		public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (ItemsView?.ItemsSource is null || !ItemsView.IsEnabled)
			{
				return;
			}

			var tap = CreateItemTappedEventArgs(indexPath);
			FormsDeselectItem(indexPath);
			QueueItemTapped(tap, selected: false);
		}

		// _Only_ called if the user initiates the selection change; will not be called for programmatic selection
		internal bool ShouldSelectItemCore(NSIndexPath indexPath)
		{
			if (ItemsView?.ItemsSource is null || !ItemsView.IsEnabled)
			{
				return false;
			}

			if (ItemsView.SelectionMode != SelectionMode.None)
			{
				return true;
			}

			// Selection is only allowed in None mode so that taps are reported; the item itself never becomes selected
			QueueItemTapped(CreateItemTappedEventArgs(indexPath), selected: true);
			return false;
		}

		ItemTappedEventArgs _pendingTap;
		bool _pendingTapSelected;
		bool _isTapQueued;

		// One click or tap can make UIKit deselect other items besides selecting the tapped one: the previous item in
		// Single mode, and on Mac Catalyst every other item in Multiple mode, because a plain click replaces the
		// selection. Collect the callbacks of one event and report a single tap: the selected item if there is one,
		// otherwise the deselected item (a tap that deselects it in Multiple mode).
		void QueueItemTapped(ItemTappedEventArgs tap, bool selected)
		{
			if (tap is null)
			{
				return;
			}

			if (_pendingTap is null || selected || !_pendingTapSelected)
			{
				_pendingTap = tap;
				_pendingTapSelected = selected;
			}

			if (!_isTapQueued)
			{
				_isTapQueued = true;
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(SendPendingItemTapped);
			}
		}

		void SendPendingItemTapped()
		{
			var tap = _pendingTap;
			_pendingTap = null;
			_isTapQueued = false;

			ItemsView?.SendItemTapped(tap);
		}

		ItemTappedEventArgs CreateItemTappedEventArgs(NSIndexPath indexPath)
		{
			if (!ItemsView.IsItemTapEnabled)
			{
				return null;
			}

			var item = GetItemAtIndex(indexPath);

			if (ItemsView is GroupableItemsView { IsGrouped: true })
			{
				return ItemsView.CreateItemTappedEventArgs(item, ItemsSource.Group(indexPath), (int)indexPath.Section, (int)indexPath.Item);
			}

			return ItemsView.CreateItemTappedEventArgs(item, null, -1, (int)indexPath.Item);
		}

		internal void UpdateItemTapEnabled()
		{
			if (ItemsView.SelectionMode == SelectionMode.None)
			{
				CollectionView.AllowsSelection = ItemsView.IsEnabled && ItemsView.IsItemTapEnabled;
			}

			CollectionView.UpdateAccessibilityTraits(ItemsView);
		}

		// Called by Forms to mark an item selected 
		internal void SelectItem(object selectedItem)
		{
			if (ItemsView?.ItemsSource is not object originalSource)
			{
				return;
			}

			var index = GetIndexForItem(selectedItem);

			if (index.Section > -1 && index.Item > -1)
			{
				// Ensure the selected index is updated after the collection view's items generation is completed
				if (!CollectionView.IsLoaded())
				{
					CollectionView.PerformBatchUpdates(null, _ =>
					{
						ValidateAndSelectItem(selectedItem, originalSource);
					});
				}
				else
				{
					ValidateAndSelectItem(selectedItem, originalSource);
				}
			}
		}

		void ValidateAndSelectItem(object selectedItem, object originalSource)
		{
			// Ensure ItemsSource hasn't been disposed
			if (ItemsSource is EmptySource)
			{
				return;
			}

			// Exit if the ItemsSource reference no longer matches the one captured at invocation.
			if (!ReferenceEquals(ItemsView.ItemsSource, originalSource))
			{
				return;
			}

			// Recalculate the index for the selectedItem now that the collection may have changed.(Adding, deleting etc..)
			var updatedIndex = GetIndexForItem(selectedItem);
			if (updatedIndex.Section < 0 || updatedIndex.Item < 0)
			{
				return;
			}

			// Retrieve the current item at that index and verify it still equals the intended selection.
			var liveItem = GetItemAtIndex(updatedIndex);
			if (!Equals(liveItem, selectedItem))
			{
				return;
			}

			CollectionView.SelectItem(updatedIndex, true, UICollectionViewScrollPosition.None);
		}

		// Called by Forms to clear the native selection
		internal void ClearSelection()
		{
			if (ItemsView?.ItemsSource is null)
			{
				return;
			}

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
			if (ItemsView?.ItemsSource is null)
			{
				return;
			}

			var mode = ItemsView.SelectionMode;

			switch (mode)
			{
				case SelectionMode.None:
					// Selection can stay enabled in None mode to report taps, so clear any selection left from a previous mode
					ClearSelection();
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
			var isEnabled = ItemsView.IsEnabled;

			switch (mode)
			{
				case SelectionMode.None:
					// Selection stays enabled while item taps are observed; ShouldSelectItemCore reports the tap and refuses the selection
					CollectionView.AllowsSelection = isEnabled && ItemsView.IsItemTapEnabled;
					CollectionView.AllowsMultipleSelection = false;
					ClearsSelectionOnViewWillAppear = true;
					break;
				case SelectionMode.Single:
					CollectionView.AllowsSelection = isEnabled;
					CollectionView.AllowsMultipleSelection = false;
					ClearsSelectionOnViewWillAppear = false;
					break;
				case SelectionMode.Multiple:
					CollectionView.AllowsSelection = isEnabled;
					CollectionView.AllowsMultipleSelection = isEnabled;
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
