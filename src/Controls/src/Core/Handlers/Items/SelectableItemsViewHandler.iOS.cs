using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		protected override ItemsViewController<TItemsView> CreateController(TItemsView itemsView, ItemsViewLayout layout)
			=> new SelectableItemsViewController<TItemsView>(itemsView, layout);

		protected override void ConnectHandler(UIView nativeView)
		{
			base.ConnectHandler(nativeView);
			UpdateSelectionMode();
			UpdateNativeSelection();
		}

		protected virtual void UpdateNativeSelection()
		{
			(Controller as SelectableItemsViewController<TItemsView>)?.UpdateNativeSelection();
		}

		protected virtual void UpdateSelectionMode()
		{
			(Controller as SelectableItemsViewController<TItemsView>)?.UpdateSelectionMode();
		}

		protected override void UpdateItemsSource()
		{
			base.UpdateItemsSource();
			UpdateNativeSelection();
		}

		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			handler.UpdateNativeSelection();
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			handler.UpdateNativeSelection();
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			handler.UpdateSelectionMode();
		}
	}
}
