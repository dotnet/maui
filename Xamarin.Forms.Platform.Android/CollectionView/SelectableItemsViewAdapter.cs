using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	public class SelectableItemsViewAdapter : ItemsViewAdapter
	{
		protected readonly SelectableItemsView SelectableItemsView;
		List<SelectableViewHolder> _currentViewHolders = new List<SelectableViewHolder>();

		internal SelectableItemsViewAdapter(SelectableItemsView selectableItemsView,
			Func<View, Context, ItemContentView> createView = null) : base(selectableItemsView, createView)
		{
			SelectableItemsView = selectableItemsView;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			base.OnBindViewHolder(holder, position);

			if (!(holder is SelectableViewHolder selectable))
			{
				return;
			}

			// Watch for clicks so the user can select the item held by this ViewHolder
			selectable.Clicked += SelectableOnClicked;

			// Keep track of the view holders here so we can clear the native selection
			_currentViewHolders.Add(selectable);

			// Make sure that if this item is one of the selected items, it's marked as selected
			selectable.IsSelected = PostionIsSelected(position);
		}
	
		public override void OnViewRecycled(Object holder)
		{
			if (holder is SelectableViewHolder selectable)
			{
				_currentViewHolders.Remove(selectable);
				selectable.Clicked -= SelectableOnClicked;
				selectable.IsSelected = false;
			}

			base.OnViewRecycled(holder);
		}

		internal void ClearNativeSelection()
		{
			for (int i = 0; i < _currentViewHolders.Count; i++)
			{
				_currentViewHolders[i].IsSelected = false;
			}
		}

		internal void MarkNativeSelection(object selectedItem)
		{
			if (selectedItem == null)
			{
				return;
			}

			var position = GetPositionForItem(selectedItem);

			for (int i = 0; i < _currentViewHolders.Count; i++)
			{
				if (_currentViewHolders[i].AdapterPosition == position)
				{
					_currentViewHolders[i].IsSelected = true;
					return;
				}
			}
		}

		int[] GetSelectedPositions()
		{
			switch (SelectableItemsView.SelectionMode)
			{
				case SelectionMode.None:
					return new int[0];

				case SelectionMode.Single:
					var selectedItem = SelectableItemsView.SelectedItem;
					if (selectedItem == null)
					{
						return new int[0];
					}

					return new int[1] { GetPositionForItem(selectedItem) };

				case SelectionMode.Multiple:
					var selectedItems = SelectableItemsView.SelectedItems;
					var result = new int[selectedItems.Count];

					for (int n = 0; n < result.Length; n++)
					{
						result[n] = GetPositionForItem(selectedItems[n]);
					}

					return result;
			}

			return new int[0];
		}

		bool PostionIsSelected(int position)
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

		void SelectableOnClicked(object sender, int adapterPosition)
		{
			UpdateFormsSelection(adapterPosition);
		}

		void UpdateFormsSelection(int adapterPosition)
		{
			var mode = SelectableItemsView.SelectionMode;

			switch (mode)
			{
				case SelectionMode.None:
					// Selection's not even on, so there's nothing to do here
					return;
				case SelectionMode.Single:
					SelectableItemsView.SelectedItem = ItemsSource[adapterPosition];
					return;
				case SelectionMode.Multiple:
					var item = ItemsSource[adapterPosition];
					var selectedItems = SelectableItemsView.SelectedItems;

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