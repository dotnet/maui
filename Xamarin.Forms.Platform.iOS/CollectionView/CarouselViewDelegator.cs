using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewDelegator : ItemsViewDelegator<CarouselView, CarouselViewController>
	{
		public CarouselViewDelegator(ItemsViewLayout itemsViewLayout, CarouselViewController itemsViewController)
			: base(itemsViewLayout, itemsViewController)
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
			if (VisibleItems)
			{
				firstVisibleItemIndex = ViewController.GetIndexFromIndexPath(First);
				centerItemIndex = ViewController.GetIndexFromIndexPath(Center);
				lastVisibleItemIndex = ViewController.GetIndexFromIndexPath(Last);
			}
			return (VisibleItems, firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}
	}
}