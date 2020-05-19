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

			(ViewController as CarouselViewController)?.UpdateIsScrolling(true);
		}

		public override void ScrollAnimationEnded(UIScrollView scrollView)
		{
			(ViewController as CarouselViewController)?.UpdateIsScrolling(false);
		}

		public override void DecelerationEnded(UIScrollView scrollView)
		{
			(ViewController as CarouselViewController)?.UpdateIsScrolling(false);
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
	}
}