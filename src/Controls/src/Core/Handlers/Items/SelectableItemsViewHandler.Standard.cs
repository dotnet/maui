using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapSelectedItem(ISelectableItemsViewHandler handler, SelectableItemsView itemsView) { }

		public static void MapSelectedItems(ISelectableItemsViewHandler handler, SelectableItemsView itemsView) { }

		public static void MapSelectionMode(ISelectableItemsViewHandler handler, SelectableItemsView itemsView) { }
	}
}
