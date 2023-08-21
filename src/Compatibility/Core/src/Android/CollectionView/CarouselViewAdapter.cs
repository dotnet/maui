//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Android.Content;
using AndroidX.RecyclerView.Widget;
#pragma warning disable CS0618 // Type or member is obsolete
using static Microsoft.Maui.Controls.Compatibility.Platform.Android.CarouselViewRenderer;
#pragma warning restore CS0618 // Type or member is obsolete

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.CollectionView
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
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
