#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> where TItemsView : GroupableItemsView
	{
		public GroupableItemsViewHandler() : base(GroupableItemsViewMapper)
		{

		}
		public GroupableItemsViewHandler(PropertyMapper mapper = null) : base(mapper ?? GroupableItemsViewMapper)
		{

		}

		public static PropertyMapper<TItemsView, GroupableItemsViewHandler<TItemsView>> GroupableItemsViewMapper = new(SelectableItemsViewMapper)
		{
			[GroupableItemsView.IsGroupedProperty.PropertyName] = MapIsGrouped,
#if WINDOWS || __ANDROID__ || TIZEN
			[GroupableItemsView.GroupFooterTemplateProperty.PropertyName] = MapIsGrouped,
			[GroupableItemsView.GroupHeaderTemplateProperty.PropertyName] = MapIsGrouped,
#endif
		};
	}
}
