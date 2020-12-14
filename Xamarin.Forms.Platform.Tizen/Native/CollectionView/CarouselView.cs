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
			SnapPointsType = SnapPointsType.MandatorySingle;
		}

		public EScroller Scroll => base.Scroller;

		ESize ICollectionViewController.GetItemSize(int widthConstraint, int heightConstraint)
		{
			return AllocatedSize;
		}
	}
}
