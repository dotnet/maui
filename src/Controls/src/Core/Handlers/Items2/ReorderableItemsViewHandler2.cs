#nullable disable
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class ReorderableItemsViewHandler2<TItemsView> where TItemsView : ReorderableItemsView
	{
		public ReorderableItemsViewHandler2() : base(ReorderableItemsViewHandler<TItemsView>.ReorderableItemsViewMapper)
		{

		}
		public ReorderableItemsViewHandler2(PropertyMapper mapper = null) : base(mapper ?? ReorderableItemsViewHandler<TItemsView>.ReorderableItemsViewMapper)
		{
		}
	}
}
