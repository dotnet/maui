using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;


namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> : SelectableItemsViewHandler<TItemsView>
		where TItemsView : GroupableItemsView
	{
		public static void MapIsGrouped(GroupableItemsViewHandler<TItemsView> handler, GroupableItemsView itemsView)
		{
		}
	}
}
