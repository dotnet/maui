#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class SelectableItemsViewHandler2<TItemsView> where TItemsView : SelectableItemsView
	{
		public SelectableItemsViewHandler2() : base(SelectableItemsViewHandler<TItemsView>.SelectableItemsViewMapper)
		{

		}

		public SelectableItemsViewHandler2(PropertyMapper mapper = null) : base(mapper ?? SelectableItemsViewHandler<TItemsView>.SelectableItemsViewMapper)
		{

		}
	}
}
