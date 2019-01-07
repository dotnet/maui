using System;
using System.ComponentModel;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	public class SelectableItemsViewRenderer : ItemsViewRenderer
	{
		SelectableItemsView SelectableItemsView => (SelectableItemsView)ItemsView;

		SelectableItemsViewAdapter SelectableItemsViewAdapter => (SelectableItemsViewAdapter)ItemsViewAdapter; 

		public SelectableItemsViewRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);
			
			if (changedProperty.Is(SelectableItemsView.SelectedItemProperty))
			{
				UpdateSelection();
			}
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			if (!(newElement is SelectableItemsView))
			{
				throw new ArgumentException($"{nameof(newElement)} must be of type {typeof(SelectableItemsView).Name}");
			}

			base.SetUpNewElement(newElement);

			UpdateSelection();
		}

		protected override void UpdateAdapter()
		{
			ItemsViewAdapter = new SelectableItemsViewAdapter(SelectableItemsView);
			SwapAdapter(ItemsViewAdapter, true);
		}

		void ClearSelection()
		{
			for (int i = 0, size = ChildCount; i < size; i++)
			{
				var holder = GetChildViewHolder(GetChildAt(i));
				
				if (holder is SelectableViewHolder selectable)
				{
					selectable.IsSelected = false;
				}
			}
		}

		void MarkItemSelected(object selectedItem)
		{
			var position = ItemsViewAdapter.GetPositionForItem(selectedItem);
			var selectedHolder = FindViewHolderForAdapterPosition(position);
			if (selectedHolder == null)
			{
				return;
			}

			if (selectedHolder is SelectableViewHolder selectable)
			{
				selectable.IsSelected = true;
			}
		}

		void UpdateSelection()
		{
			var mode = SelectableItemsView.SelectionMode;
			var selectedItem = SelectableItemsView.SelectedItem;

			if (selectedItem == null)
			{
				if (mode == SelectionMode.None || mode == SelectionMode.Single)
				{
					ClearSelection();
				}

				// If the mode is Multiple and SelectedItem is set to null, don't do anything
				return;
			}

			if (mode != SelectionMode.Multiple)
			{
				ClearSelection();
				MarkItemSelected(selectedItem);
			}

			// TODO hartez 2018/11/06 22:32:07 This doesn't cover all the possible cases yet; need to handle multiple selection	
		}
	}
}