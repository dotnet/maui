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

			if (changedProperty.IsOneOf(SelectableItemsView.SelectedItemProperty, 
				SelectableItemsView.SelectedItemsProperty, 
				SelectableItemsView.SelectionModeProperty))
			{
				UpdateNativeSelection();
			}
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			if (newElement != null && !(newElement is SelectableItemsView))
			{
				throw new ArgumentException($"{nameof(newElement)} must be of type {typeof(SelectableItemsView).Name}");
			}

			base.SetUpNewElement(newElement);

			UpdateNativeSelection();
		}

		protected override void UpdateAdapter()
		{
			ItemsViewAdapter = new SelectableItemsViewAdapter(SelectableItemsView);
			SwapAdapter(ItemsViewAdapter, true);
		}

		void UpdateNativeSelection()
		{
			var mode = SelectableItemsView.SelectionMode;

			SelectableItemsViewAdapter.ClearNativeSelection();

			switch (mode)
			{
				case SelectionMode.None:
					return;

				case SelectionMode.Single:
					var selectedItem = SelectableItemsView.SelectedItem;
					SelectableItemsViewAdapter.MarkNativeSelection(selectedItem);
					return;

				case SelectionMode.Multiple:
					var selectedItems = SelectableItemsView.SelectedItems;
					
					foreach(var item in selectedItems)
					{
						SelectableItemsViewAdapter.MarkNativeSelection(item);
					}
					return;
			}
		}
	}
}