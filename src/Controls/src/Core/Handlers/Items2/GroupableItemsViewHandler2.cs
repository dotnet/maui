#nullable disable
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class GroupableItemsViewHandler2<TItemsView> where TItemsView : GroupableItemsView
	{
		public GroupableItemsViewHandler2() : base(GroupableItemsViewHandler<TItemsView>.GroupableItemsViewMapper)
		{

		}
		public GroupableItemsViewHandler2(PropertyMapper mapper = null) : base(mapper ?? GroupableItemsViewHandler<TItemsView>.GroupableItemsViewMapper)
		{

		}
	}
}
