using System;
using Android.Content;
#if __ANDROID_29__
using AndroidX.RecyclerView.Widget;
using static Xamarin.Forms.Platform.Android.CarouselViewRenderer;
#else
using Android.Support.V7.Widget;
#endif

namespace Xamarin.Forms.Platform.Android.CollectionView
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

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (CarouselView == null || ItemsSource == null)
				return;

			bool hasItems = ItemsSource != null && ItemsSource.Count > 0;

			if (!hasItems)
				return;

			int positionInList = (CarouselView.Loop && hasItems) ? position % ItemsSource.Count : position;

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
	}
}
