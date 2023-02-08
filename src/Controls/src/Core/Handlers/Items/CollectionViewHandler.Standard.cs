#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CollectionViewHandler : ReorderableItemsViewHandler<ReorderableItemsView>
	{
		protected override object CreatePlatformView()
		{
			throw new NotImplementedException();
		}
	}
}
