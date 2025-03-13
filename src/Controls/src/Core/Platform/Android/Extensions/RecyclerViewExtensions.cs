#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Handlers.Items;
using AndroidX.Core.View;
using Android.Views;
using AView = Android.Views.View;
using AViewAccessibility = Android.Views.Accessibility;
using AWidget = Android.Widget;
using ARecyclerWidget = AndroidX.RecyclerView.Widget;


namespace Microsoft.Maui.Controls.Platform
{
	public static class RecyclerViewExtensions
	{
		public static void UpdateSelection(this RecyclerView recyclerView, SelectableItemsView selectableItemsView)
		{
			var mode = selectableItemsView.SelectionMode;
			//TODO: on NET8 implement a ISelectableItemsViewAdapter interface on the adapter
			var adapter = recyclerView.GetAdapter() as ReorderableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>;
			if (adapter == null)
				return;

			adapter.ClearPlatformSelection();

			switch (mode)
			{
				case SelectionMode.None:
					return;

				case SelectionMode.Single:
					var selectedItem = selectableItemsView.SelectedItem;
					adapter.MarkPlatformSelection(selectedItem);
					return;

				case SelectionMode.Multiple:
					var selectedItems = selectableItemsView.SelectedItems;
					foreach (var item in selectedItems)
					{
						adapter.MarkPlatformSelection(item);
					}
					return;
			}
		}

		internal static void UpdateItemAccessibilityEvents(this RecyclerView recyclerView, SelectableItemsView selectableItemsView, ViewHandler handler)
		{
			var adapter = recyclerView.GetAdapter() as ReorderableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>;
			if (adapter?.ItemsSource == null)
				return;

			for (int i = 0; i < adapter.ItemCount; i++)
			{
				var viewHolder = recyclerView.FindViewHolderForAdapterPosition(i);
				var itemView = viewHolder?.ItemView;

				itemView.AddRecyclerItemViewAccessibility(selectableItemsView.SelectionMode != SelectionMode.None);
			}
		}
	}
}
