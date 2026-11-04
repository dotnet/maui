using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CollectionViewHandler : ReorderableItemsViewHandler<ReorderableItemsView>
	{
		internal override bool PreventGestureBubbling => VirtualView.SelectionMode is not SelectionMode.None;
	}
}
