using ElmSharp;
using EScroller = ElmSharp.Scroller;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class CarouselView : CollectionView, ICollectionViewController
	{
		public CarouselView(EvasObject parent) : base(parent)
		{
			SelectionMode = CollectionViewSelectionMode.Single;
			Scroll.ScrollBlock = ScrollBlock.None;
			Scroll.HorizontalPageScrollLimit = 1;
			Scroll.VerticalPageScrollLimit = 1;
			Scroll.SetPageSize(1.0, 1.0);
			Scroll.HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible;
			Scroll.VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible;
		}

		public EScroller Scroll => base.Scroller;

		protected override ViewHolder CreateViewHolder()
		{
			return new ViewHolder(this)
			{
				FocusedColor = ElmSharp.Color.Transparent,
				SelectedColor = ElmSharp.Color.Transparent,
			};
		}

		ESize ICollectionViewController.GetItemSize(int widthConstraint, int heightConstraint)
		{
			return AllocatedSize;
		}
	}
}
