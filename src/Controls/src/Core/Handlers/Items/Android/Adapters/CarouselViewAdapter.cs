#nullable disable
using System;
using Android.Content;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class CarouselViewAdapter<TItemsView, TItemsViewSource> : ItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsView : ItemsView
		where TItemsViewSource : IItemsViewSource
	{
		protected readonly CarouselView CarouselView;
		internal CarouselViewAdapter(CarouselView itemsView, Func<View, Context, ItemContentView> createItemContentView = null) : base(itemsView as TItemsView, createItemContentView)
		{
			CarouselView = itemsView;
		}

		public override int ItemCount => CarouselView.Loop && !(ItemsSource is EmptySource)
			&& ItemsSource.Count > 0 ? CarouselViewLoopManager.LoopScale : ItemsSource.Count;

		protected override bool IsSelectionEnabled(global::Android.Views.ViewGroup parent, int viewType) => false;

		public override int GetItemViewType(int position)
		{
			int positionInList = GetPositionInList(position);

			if (positionInList < 0)
				return ItemViewType.TextItem;

			return base.GetItemViewType(positionInList);
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			int positionInList = GetPositionInList(position);

			if (positionInList < 0)
				return;

			switch (holder)
			{
				case TextViewHolder textViewHolder:
					textViewHolder.TextView.Text = ItemsSource.GetItem(positionInList).ToString();
					break;
				case TemplatedItemViewHolder templatedItemViewHolder:
					BindTemplatedItemViewHolder(templatedItemViewHolder, ItemsSource.GetItem(positionInList));
					break;
			}
		}

		int GetPositionInList(int position)
		{
			if (CarouselView == null || ItemsSource == null)
				return -1;

			bool hasItems = ItemsSource != null && ItemsSource.Count > 0;

			if (!hasItems)
				return -1;

			return (CarouselView.Loop && hasItems) ? position % ItemsSource.Count : position;
		}
	}
}
