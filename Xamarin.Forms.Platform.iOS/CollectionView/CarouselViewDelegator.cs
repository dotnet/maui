using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewDelegator : ItemsViewDelegator<CarouselView, CarouselViewController>
	{
		public CarouselViewDelegator(ItemsViewLayout itemsViewLayout, CarouselViewController itemsViewController) 
			: base(itemsViewLayout, itemsViewController)
		{
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