#nullable disable
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class SelectableItemsViewAdapter<TItemsView, TItemsSource> : StructuredItemsViewAdapter<TItemsView, TItemsSource>
		where TItemsView : SelectableItemsView
		where TItemsSource : IItemsViewSource
	{
		List<SelectableViewHolder> _currentViewHolders = new List<SelectableViewHolder>();
		HashSet<object> _selectedSet = new HashSet<object>();
		// Adapter position of the null data item that is currently selected, or -1 if none.
		// Needed because SelectedItem==null is ambiguous ("nothing selected" vs "null item selected").
		int _selectedNullPosition = -1;

		protected internal SelectableItemsViewAdapter(TItemsView selectableItemsView,
			Func<View, Context, ItemContentView> createView = null) : base(selectableItemsView, createView)
		{
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			base.OnBindViewHolder(holder, position);

			if (!(holder is SelectableViewHolder selectable))
			{
				return;
			}

			// Header and footer view holders should not participate in selection tracking.
			// They are not data items and calling GetItem() on their positions would cause
			// an ArgumentOutOfRangeException due to the header index adjustment.
			if (ItemsSource.IsHeader(position) || ItemsSource.IsFooter(position))
			{
				return;
			}

			// Watch for clicks so the user can select the item held by this ViewHolder
			selectable.Clicked += SelectableClicked;

			// Keep track of the view holders here so we can clear the native selection
			_currentViewHolders.Add(selectable);

			// Make sure that if this item is one of the selected items, it's marked as selected
			selectable.IsSelected = PositionIsSelected(position);
		}

		public override void OnViewRecycled(Object holder)
		{
			if (holder is SelectableViewHolder selectable)
			{
				_currentViewHolders.Remove(selectable);
				selectable.Clicked -= SelectableClicked;
				selectable.IsSelected = false;
			}

			base.OnViewRecycled(holder);
		}

		internal void ClearPlatformSelection()
		{
			for (int i = 0; i < _currentViewHolders.Count; i++)
			{
				_currentViewHolders[i].IsSelected = false;
			}
		}

		internal void UpdateSelectionMode()
		{
			// Update click listeners for all currently visible ViewHolders when SelectionMode changes
			bool selectionEnabled = ItemsView.SelectionMode is not SelectionMode.None;
			for (int i = 0; i < _currentViewHolders.Count; i++)
			{
				_currentViewHolders[i].UpdateClickListener(selectionEnabled);
			}
		}

		internal void MarkPlatformSelection(SelectableItemsView selectableItemsView)
		{
			if (_currentViewHolders.Count == 0)
			{
				return;
			}

			_selectedSet.Clear();

			switch (selectableItemsView.SelectionMode)
			{
				case SelectionMode.None:
					_selectedNullPosition = -1;
					ClearPlatformSelection();
					return;

				case SelectionMode.Single:
					var selectedItem = selectableItemsView.SelectedItem;
					if (selectedItem == null)
					{
						ClearPlatformSelection();
						// Restore the null-item selection if one was explicitly selected.
						// _selectedNullPosition is set before UpdateMauiSelection is called,
						// so it's already valid here even though SelectedItem is null.
						if (_selectedNullPosition >= 0)
						{
							var nullHolder = _currentViewHolders.Find(h => h.BindingAdapterPosition == _selectedNullPosition);
							if (nullHolder != null)
								nullHolder.IsSelected = true;
						}
						return;
					}
					_selectedNullPosition = -1;
					_selectedSet.Add(selectedItem);
					break;

				case SelectionMode.Multiple:
					_selectedNullPosition = -1;
					var selectedItems = selectableItemsView.SelectedItems;
					if (selectedItems == null || selectedItems.Count == 0)
					{
						ClearPlatformSelection();
						return;
					}

					_selectedSet.UnionWith(selectedItems);
					break;

				default:
					return;
			}

			for (int i = 0; i < _currentViewHolders.Count; i++)
			{
				var holder = _currentViewHolders[i];
				if (holder.BindingAdapterPosition >= 0)
				{
					var item = ItemsSource.GetItem(holder.BindingAdapterPosition);
					// Guard: null data items must never match _selectedSet even if null was
					// inadvertently added (e.g., via multi-select). If null were matched,
					// every null-bound ViewHolder would appear selected (false positive)
					// because HashSet.Contains(null) returns true for all of them.
					bool shouldBeSelected = item is not null && _selectedSet.Contains(item);

					if (holder.IsSelected != shouldBeSelected)
					{
						holder.IsSelected = shouldBeSelected;
					}
				}
			}
		}

		int[] GetSelectedPositions()
		{
			switch (ItemsView.SelectionMode)
			{
				case SelectionMode.None:
					return Array.Empty<int>();

				case SelectionMode.Single:
					var selectedItem = ItemsView.SelectedItem;
					if (selectedItem == null)
					{
						return Array.Empty<int>();
					}

					return new int[1] { GetPositionForItem(selectedItem) };

				case SelectionMode.Multiple:
					var selectedItems = ItemsView.SelectedItems;
					var result = new int[selectedItems.Count];

					for (int n = 0; n < result.Length; n++)
					{
						result[n] = GetPositionForItem(selectedItems[n]);
					}

					return result;
			}

			return Array.Empty<int>();
		}

		protected override bool IsSelectionEnabled(ViewGroup parent, int viewType) 
		{
			if (ItemsView == null)
			{
				return false;
			}
			// Disable click listeners when SelectionMode is None to prevent TalkBack from announcing items as clickable
			return ItemsView.SelectionMode != SelectionMode.None;
		}

		bool PositionIsSelected(int position)
		{
			// A null item is selected at a specific position tracked separately.
			if (position == _selectedNullPosition)
				return true;

			var selectedPositions = GetSelectedPositions();
			foreach (var selectedPosition in selectedPositions)
			{
				if (selectedPosition == position)
				{
					return true;
				}
			}

			return false;
		}

		void SelectableClicked(object sender, int adapterPosition)
		{
			if (adapterPosition >= 0 && adapterPosition < ItemsSource?.Count)
			{
				var clickedItem = ItemsSource.GetItem(adapterPosition);

				// Set _selectedNullPosition BEFORE UpdateMauiSelection so that the synchronous
				// MarkPlatformSelection call (fired by SelectedItemPropertyChanged) can restore
				// the null-item selection highlight immediately after ClearPlatformSelection.
				if (clickedItem is null && ItemsView.SelectionMode == SelectionMode.Single)
					_selectedNullPosition = adapterPosition;
				else
					_selectedNullPosition = -1;

				UpdateMauiSelection(adapterPosition);
				// Unconditionally sync visual state for Single mode.
				// Handles value-equal items where PropertyChanged is suppressed,
				// and null data items whose selection cannot be inferred from SelectedItem alone.
				if (ItemsView.SelectionMode == SelectionMode.Single && sender is SelectableViewHolder clickedHolder)
				{
					// If the clicked item was non-null but SelectedItem is now null, the user
					// deselected via AllowDeselection. Don't re-apply the visual selection.
					bool isDeselect = clickedItem is not null && ItemsView.SelectedItem is null;
					if (!isDeselect)
					{
						ClearPlatformSelection();
						clickedHolder.IsSelected = true;
					}
				}
			}
		}

		void UpdateMauiSelection(int adapterPosition)
		{
			var mode = ItemsView.SelectionMode;

			switch (mode)
			{
				case SelectionMode.None:
					// Selection's not even on, so there's nothing to do here
					return;
				case SelectionMode.Single:
					ItemsView.SelectedItem = ItemsSource.GetItem(adapterPosition);
					return;
				case SelectionMode.Multiple:
					var item = ItemsSource.GetItem(adapterPosition);
					var selectedItems = ItemsView.SelectedItems;

					if (selectedItems.Contains(item))
					{
						selectedItems.Remove(item);
					}
					else
					{
						selectedItems.Add(item);
					}
					return;
			}
		}
	}
}
