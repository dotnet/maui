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

		// HeaderProperty and HeaderTemplateProperty (and the Footer equivalents) both route here, since the
		// shared PropertyMapper maps them to the same action. _lastHeaderTemplate/_lastFooterTemplate let us
		// tell a template swap (needs a full adapter rebuild) apart from a plain content change (doesn't).
		DataTemplate _lastHeaderTemplate;
		DataTemplate _lastFooterTemplate;

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeaderFooter(true);
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeaderFooter(false);
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			if (handler.PlatformView is IMauiRecyclerView<TItemsView> recyclerView)
			{
				recyclerView.UpdateAdapter();
				recyclerView.UpdateScrollingMode();
				recyclerView.UpdateLayoutManager();
			}
		}

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

			var currentTemplate = isHeader ? VirtualView.HeaderTemplate : VirtualView.FooterTemplate;

			bool hasHeaderOrFooter = isHeader
			? (VirtualView.Header ?? VirtualView.HeaderTemplate) != null
			: (VirtualView.Footer ?? VirtualView.FooterTemplate) != null;

			bool exists = isHeader
			? DoesHeaderOrFooterExist(adapter, position: 0, ItemViewType.Header)
			: DoesHeaderOrFooterExist(adapter, position: adapter.ItemCount - 1, ItemViewType.Footer);

			bool templateChanged = isHeader
			? !ReferenceEquals(_lastHeaderTemplate, currentTemplate)
			: !ReferenceEquals(_lastFooterTemplate, currentTemplate);

			if (isHeader)
			{
				_lastHeaderTemplate = currentTemplate;
			}
			else
			{
				_lastFooterTemplate = currentTemplate;
			}

			if (hasHeaderOrFooter != exists)
			{
				// Header/footer was added or removed - item positions shift, so the adapter needs a full rebuild.
				recyclerView.UpdateAdapter();
			}
			else if (hasHeaderOrFooter && exists && templateChanged)
			{
				// The template itself changed - the existing ViewHolder type may no longer apply.
				recyclerView.UpdateAdapter();
			}

			// Otherwise this is a content-only change (same template, header/footer already present);
			// StructuredItemsViewAdapter.ItemsViewPropertyChanged already rebinds just that position.
		}

		bool DoesHeaderOrFooterExist(RecyclerView.Adapter adapter, int position, int expectedViewType)
		{
			if (position < 0 || position >= adapter.ItemCount)
			{
				return false;
			}

			try
			{
				return adapter.GetItemViewType(position) == expectedViewType;
			}
			catch
			{
				return false;
			}
		}
	}
}
