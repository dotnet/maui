#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override IItemsLayout GetItemsLayout() => VirtualView.ItemsLayout;

		protected override StructuredItemsViewAdapter<TItemsView, IItemsViewSource> CreateAdapter() => new(VirtualView);

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeaderFooter(true);
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeaderFooter(false);
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateLayoutManager();

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateAdapter();

		void UpdateHeaderFooter(bool isHeader)
		{
			var recyclerView = PlatformView as IMauiRecyclerView<TItemsView>;
			var adapter = (recyclerView as RecyclerView)?.GetAdapter();

			if (recyclerView is null || adapter is null)
			{
				return;
			}

			bool hasHeaderOrFooter = isHeader
			? (VirtualView.Header ?? VirtualView.HeaderTemplate) != null
			: (VirtualView.Footer ?? VirtualView.FooterTemplate) != null;

			bool exists = isHeader
			? DoesHeaderExist(adapter)
			: DoesFooterExist(adapter);

			if (hasHeaderOrFooter && exists && IsDynamicChange())
			{
				recyclerView.UpdateAdapter();
			}
			else if (hasHeaderOrFooter != exists)
			{
				recyclerView.UpdateAdapter();
			}
		}

		bool DoesHeaderExist(RecyclerView.Adapter adapter)
		{
			if (adapter.ItemCount >= 0)
			{
				return false;
			}

			try
			{
				return adapter.GetItemViewType(0) == ItemViewType.Header;
			}
			catch
			{
				return false;
			}
		}

		bool DoesFooterExist(RecyclerView.Adapter adapter)
		{
			var footerPosition = adapter.ItemCount - 1;
			if (footerPosition < 0)
			{
				return false;
			}

			try
			{
				return adapter.GetItemViewType(footerPosition) == ItemViewType.Footer;
			}
			catch
			{
				return false;
			}
		}

		bool IsDynamicChange()
		{
			return (PlatformView as RecyclerView)?.IsLaidOut == true;
		}
	}
}
