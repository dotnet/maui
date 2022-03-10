using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapHeaderTemplate(IStructuredItemsViewHandler handler, StructuredItemsView itemsView) { }

		public static void MapFooterTemplate(IStructuredItemsViewHandler handler, StructuredItemsView itemsView) { }

		public static void MapItemsLayout(IStructuredItemsViewHandler handler, StructuredItemsView itemsView) { }

		public static void MapItemSizingStrategy(IStructuredItemsViewHandler handler, StructuredItemsView itemsView) { }
	}
}
