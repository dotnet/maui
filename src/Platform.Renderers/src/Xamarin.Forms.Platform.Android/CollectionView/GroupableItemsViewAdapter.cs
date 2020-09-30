using System;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class GroupableItemsViewAdapter<TItemsView, TItemsViewSource> : SelectableItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsView : GroupableItemsView
		where TItemsViewSource : IGroupableItemsViewSource
	{
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
				BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsSource.GetItem(position));
			}

			base.OnBindViewHolder(holder, position);
		}
	}
}