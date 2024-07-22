#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public abstract partial class ItemsViewHandler2<TItemsView> where TItemsView : ItemsView
	{
		public ItemsViewHandler2() : base(ItemsViewHandler<TItemsView>.ItemsViewMapper)
		{
		}

		public ItemsViewHandler2(PropertyMapper mapper = null) : base(mapper ?? ItemsViewHandler<TItemsView>.ItemsViewMapper)
		{
		}
	}
}
