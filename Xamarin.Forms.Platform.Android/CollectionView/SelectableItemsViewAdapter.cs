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
			Func<IVisualElementRenderer, Context, global::Android.Views.View> createView = null) : base(selectableItemsView, createView)
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

			var selectedItem = SelectableItemsView.SelectedItem;
			if (selectedItem == null)
			{
				return;
			}

			// If there's a selected item, check to see if it's this one so we can mark it 'selected'
			if (GetPositionForItem(selectedItem) == position)
			{
				selectable.IsSelected = true;
			}
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
					// TODO hartez 2018/11/06 22:22:42 Once SelectedItems is available, toggle ItemsSource[adapterPosition] here	
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}