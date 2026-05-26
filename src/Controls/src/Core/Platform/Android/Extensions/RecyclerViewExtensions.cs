#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Platform
{
	public static class RecyclerViewExtensions
	{
		public static void UpdateSelection(this RecyclerView recyclerView, SelectableItemsView selectableItemsView)
		{
			//TODO: on NET8 implement a ISelectableItemsViewAdapter interface on the adapter
			var adapter = recyclerView.GetAdapter() as ReorderableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>;
			if (adapter == null)
				return;

			adapter.MarkPlatformSelection(selectableItemsView);
		}
	}
}
