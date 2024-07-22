#nullable disable
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class StructuredItemsViewHandler2<TItemsView> where TItemsView : StructuredItemsView
	{
		public StructuredItemsViewHandler2() : base(StructuredItemsViewHandler<TItemsView>.StructuredItemsViewMapper)
		{
		}

		public StructuredItemsViewHandler2(PropertyMapper mapper = null) : base(mapper ?? StructuredItemsViewHandler<TItemsView>.StructuredItemsViewMapper)
		{
		}
	}
}
