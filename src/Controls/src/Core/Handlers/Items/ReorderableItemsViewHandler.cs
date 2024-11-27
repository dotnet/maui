#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class ReorderableItemsViewHandler<TItemsView> where TItemsView : ReorderableItemsView
	{
		public ReorderableItemsViewHandler() : base(ReorderableItemsViewMapper)
		{

		}
		public ReorderableItemsViewHandler(PropertyMapper mapper = null) : base(mapper ?? ReorderableItemsViewMapper)
		{

		}

		public static PropertyMapper<TItemsView, ReorderableItemsViewHandler<TItemsView>> ReorderableItemsViewMapper = new(GroupableItemsViewMapper)
		{
			[ReorderableItemsView.CanReorderItemsProperty.PropertyName] = MapCanReorderItems,
		};
	}
}
