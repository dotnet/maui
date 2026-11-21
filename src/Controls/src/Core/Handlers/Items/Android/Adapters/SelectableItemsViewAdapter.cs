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
		WeakReference<RecyclerView> _recyclerView;

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

			// Recycled ViewHolders may have been created before SelectionMode or item tapping changed
			selectable.UpdateClickListener(IsClickEnabled(selectable));

			// Watch for clicks so the user can select the item held by this ViewHolder.
			// A visible holder can be rebound without being recycled first, so never subscribe twice.
			selectable.Clicked -= SelectableClicked;
			selectable.Clicked += SelectableClicked;

			// Keep track of the view holders here so we can clear the native selection
			if (!_currentViewHolders.Contains(selectable))
			{
				_currentViewHolders.Add(selectable);
			}

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
			// Update click listeners for all currently visible ViewHolders when SelectionMode or item tapping changes
			for (int i = 0; i < _currentViewHolders.Count; i++)
			{
				_currentViewHolders[i].UpdateClickListener(IsClickEnabled(_currentViewHolders[i]));
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
					ClearPlatformSelection();
					return;

				case SelectionMode.Single:
					var selectedItem = selectableItemsView.SelectedItem;
					if (selectedItem == null)
					{
						ClearPlatformSelection();
						return;
					}

					_selectedSet.Add(selectedItem);
					break;

				case SelectionMode.Multiple:
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
					bool shouldBeSelected = _selectedSet.Contains(item);

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
			// Disable click listeners when items can be neither selected nor tapped to prevent TalkBack from announcing items as clickable
			return IsItemClickEnabled;
		}

		bool IsItemClickEnabled => ItemsView.SelectionMode != SelectionMode.None || ItemsView.IsItemTapEnabled;

		// Goes through IsSelectionEnabled so that adapters overriding it keep control over which items are clickable
		bool IsClickEnabled(SelectableViewHolder holder)
		{
			RecyclerView recyclerView = null;
			_recyclerView?.TryGetTarget(out recyclerView);
			var parent = holder.ItemView.Parent as ViewGroup ?? recyclerView;

			return IsSelectionEnabled(parent, holder.ItemViewType);
		}

		public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
		{
			base.OnAttachedToRecyclerView(recyclerView);
			_recyclerView = new WeakReference<RecyclerView>(recyclerView);
		}

		public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
		{
			_recyclerView = null;
			base.OnDetachedFromRecyclerView(recyclerView);
		}

		bool PositionIsSelected(int position)
		{
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
				var tap = CreateItemTappedEventArgs(adapterPosition);

        		UpdateMauiSelection(adapterPosition);
        		// Unconditionally sync visual state for Single mode.
        		// Handles value-equal items where PropertyChanged is suppressed.
        		if (ItemsView.SelectedItem is not null && ItemsView.SelectionMode == SelectionMode.Single && sender is SelectableViewHolder clickedHolder)
        		{
            		ClearPlatformSelection();
            		clickedHolder.IsSelected = true;
        		}

				ItemsView.SendItemTapped(tap);
    		}
		}

		ItemTappedEventArgs CreateItemTappedEventArgs(int adapterPosition)
		{
			if (!ItemsView.IsItemTapEnabled)
			{
				return null;
			}

			var item = ItemsSource.GetItem(adapterPosition);

			if (ItemsView is GroupableItemsView { IsGrouped: true } && ItemsSource is IGroupableItemsViewSource groupableSource)
			{
				var (groupIndex, indexInGroup) = groupableSource.GetGroupAndIndex(adapterPosition);

				if (groupIndex >= 0)
				{
					// Positions within a group include the group header, when there is one
					if (groupableSource.GetGroupItemsViewSource(groupIndex)?.HasHeader == true)
					{
						indexInGroup--;
					}

					return ItemsView.CreateItemTappedEventArgs(item, groupableSource.GetGroup(groupIndex), groupIndex, indexInGroup);
				}
			}

			return ItemsView.CreateItemTappedEventArgs(item, null, -1, adapterPosition - (ItemsSource.HasHeader ? 1 : 0));
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
