//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System.ComponentModel;
using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
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
