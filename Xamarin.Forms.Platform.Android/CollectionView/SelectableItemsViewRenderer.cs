using System.ComponentModel;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	public class SelectableItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource> : StructuredItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource>
		where TItemsView : SelectableItemsView
		where TAdapter : SelectableItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsViewSource : IItemsViewSource
	{
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

		protected override void SetUpNewElement(TItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			UpdateNativeSelection();
		}

		protected override TAdapter CreateAdapter()
		{
			return (TAdapter)new SelectableItemsViewAdapter<TItemsView, TItemsViewSource>(ItemsView);
		}

		void UpdateNativeSelection()
		{
			var mode = ItemsView.SelectionMode;

			ItemsViewAdapter.ClearNativeSelection();

			switch (mode)
			{
				case SelectionMode.None:
					return;

				case SelectionMode.Single:
					var selectedItem = ItemsView.SelectedItem;
					ItemsViewAdapter.MarkNativeSelection(selectedItem);
					return;

				case SelectionMode.Multiple:
					var selectedItems = ItemsView.SelectedItems;

					foreach (var item in selectedItems)
					{
						ItemsViewAdapter.MarkNativeSelection(item);
					}
					return;
			}
		}
	}
}