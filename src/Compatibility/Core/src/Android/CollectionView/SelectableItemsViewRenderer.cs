using System.ComponentModel;
using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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
				UpdatePlatformSelection();
			}
		}

		protected override void SetUpNewElement(TItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			UpdatePlatformSelection();
		}

		protected override TAdapter CreateAdapter()
		{
			return (TAdapter)new SelectableItemsViewAdapter<TItemsView, TItemsViewSource>(ItemsView);
		}

		void UpdatePlatformSelection()
		{
			var mode = ItemsView.SelectionMode;

			ItemsViewAdapter.ClearPlatformSelection();

			switch (mode)
			{
				case SelectionMode.None:
					return;

				case SelectionMode.Single:
					var selectedItem = ItemsView.SelectedItem;
					ItemsViewAdapter.MarkPlatformSelection(selectedItem);
					return;

				case SelectionMode.Multiple:
					var selectedItems = ItemsView.SelectedItems;

					foreach (var item in selectedItems)
					{
						ItemsViewAdapter.MarkPlatformSelection(item);
					}
					return;
			}
		}
	}
}
