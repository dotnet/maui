#nullable disable
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class CarouselViewDelegator2 : ItemsViewDelegator2<CarouselView, CarouselViewController2>
	{
		public CarouselViewDelegator2(UICollectionViewLayout itemsViewLayout, CarouselViewController2 ItemsViewController2)
			: base(itemsViewLayout, ItemsViewController2)
		{
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			base.Scrolled(scrollView);

			ViewController?.UpdateIsScrolling(true);
		}

		public override void ScrollAnimationEnded(UIScrollView scrollView)
		{
			ViewController?.UpdateIsScrolling(false);
		}

		public override void DecelerationEnded(UIScrollView scrollView)
		{
			ViewController?.UpdateIsScrolling(false);
		}

		public override void DraggingStarted(UIScrollView scrollView)
		{
			ViewController?.DraggingStarted(scrollView);

			PreviousHorizontalOffset = (float)scrollView.ContentOffset.X;
			PreviousVerticalOffset = (float)scrollView.ContentOffset.Y;
		}

		public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
		{
			PreviousHorizontalOffset = 0;
			PreviousVerticalOffset = 0;

			ViewController?.DraggingEnded(scrollView, willDecelerate);
		}

		protected override (bool VisibleItems, int First, int Center, int Last) GetVisibleItemsIndex()
		{
			var (VisibleItems, First, Center, Last) = GetVisibleItemsIndexPath();
			int firstVisibleItemIndex = -1, centerItemIndex = -1, lastVisibleItemIndex = -1;
			if (VisibleItems && ViewController is CarouselViewController2 vc)
			{
				firstVisibleItemIndex = vc.GetIndexFromIndexPath(First);
				centerItemIndex = vc.GetIndexFromIndexPath(Center);
				lastVisibleItemIndex = vc.GetIndexFromIndexPath(Last);
			}
			return (VisibleItems, firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}
	}
}