using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override Android.Views.View CreateNativeView()
		{
			return new StructuredItemsViewAdapter<TItemsView, IItemsViewSource>(VirtualView);
		}

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{

		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{

		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{

		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{

		}

		protected override IItemsLayout GetItemsLayout()
		{
			return VirtualView.ItemsLayout;
		}
	}
}
