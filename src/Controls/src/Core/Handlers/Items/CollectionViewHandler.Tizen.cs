using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CollectionViewHandler : GroupableItemsViewHandler<GroupableItemsView>
	{
		
		protected override Tizen.UIExtensions.ElmSharp.CollectionView CreateNativeView()
		{
			throw new NotImplementedException();
		}
	}
}
