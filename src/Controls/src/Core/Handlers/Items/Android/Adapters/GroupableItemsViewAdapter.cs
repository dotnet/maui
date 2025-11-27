#nullable disable
using System;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class GroupableItemsViewAdapter<TItemsView, TItemsViewSource> : SelectableItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsView : GroupableItemsView
		where TItemsViewSource : IGroupableItemsViewSource
	{
		bool _isBindingGroupHeaderOrFooter;

		protected internal GroupableItemsViewAdapter(TItemsView groupableItemsView,
			Func<View, Context, ItemContentView> createView = null) : base(groupableItemsView, createView)
		{
		}

		protected override TItemsViewSource CreateItemsSource()
		{
			return (TItemsViewSource)ItemsSourceFactory.Create(ItemsView, this);
		}

		public override int GetItemViewType(int position)
		{
			if (ItemsSource.IsGroupHeader(position))
			{
				return ItemViewType.GroupHeader;
			}

			if (ItemsSource.IsGroupFooter(position))
			{
				return ItemViewType.GroupFooter;
			}

			return base.GetItemViewType(position);
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			if (viewType == ItemViewType.GroupHeader)
			{
				var itemContentView = new ItemContentView(context);
				return new TemplatedItemViewHolder(itemContentView, ItemsView.GroupHeaderTemplate, isSelectionEnabled: false);
			}

			if (viewType == ItemViewType.GroupFooter)
			{
				var itemContentView = new ItemContentView(context);
				return new TemplatedItemViewHolder(itemContentView, ItemsView.GroupFooterTemplate, isSelectionEnabled: false);
			}

			return base.OnCreateViewHolder(parent, viewType);
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (holder is TemplatedItemViewHolder templatedItemViewHolder &&
				(ItemsSource.IsGroupFooter(position) || ItemsSource.IsGroupHeader(position)))
			{
				// Group headers and footers should always measure themselves,
				// not participate in the ItemSizingStrategy.MeasureFirstItem logic.
				// Set flag to bypass MeasureFirstItem in BindTemplatedItemViewHolder.
				_isBindingGroupHeaderOrFooter = true;
				try
				{
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsSource.GetItem(position));
				}
				finally
				{
					_isBindingGroupHeaderOrFooter = false;
				}
				return;
			}

			base.OnBindViewHolder(holder, position);
		}

		protected override void BindTemplatedItemViewHolder(TemplatedItemViewHolder templatedItemViewHolder, object context)
		{
			// If binding a group header or footer, skip MeasureFirstItem logic
			// and use the base implementation which always measures the item
			if (_isBindingGroupHeaderOrFooter)
			{
				// Call the grandparent's implementation (ItemsViewAdapter.BindTemplatedItemViewHolder)
				// which doesn't apply MeasureFirstItem sizing
				templatedItemViewHolder.Bind(context, ItemsView);
				return;
			}

			// For regular items, use parent's MeasureFirstItem logic
			base.BindTemplatedItemViewHolder(templatedItemViewHolder, context);
		}
	}
}
