using System.ComponentModel;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView, CarouselViewController>
	{
		CarouselView Carousel => Element;

		[Preserve(Conditional = true)]
		public CarouselViewRenderer()
		{
			CarouselView.VerifyCarouselViewFlagEnabled(nameof(CarouselViewRenderer));
		}

		protected override CarouselViewController CreateController(CarouselView newElement, ItemsViewLayout layout)
		{
			return new CarouselViewController(newElement, layout);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(CarouselView.PeekAreaInsetsProperty))
			{
				(Controller.Layout as CarouselViewLayout).UpdateConstraints(Frame.Size);
				Controller.Layout.InvalidateLayout();
			}
			else if (changedProperty.Is(CarouselView.IsSwipeEnabledProperty))
				UpdateIsSwipeEnabled();
			else if (changedProperty.Is(CarouselView.IsBounceEnabledProperty))
				UpdateIsBounceEnabled();
		}

		protected override ItemsViewLayout SelectLayout()
		{
			return new CarouselViewLayout(Carousel.ItemsLayout, Carousel);
		}

		protected override void SetUpNewElement(CarouselView newElement)
		{
			base.SetUpNewElement(newElement);
			UpdateIsSwipeEnabled();
			UpdateIsBounceEnabled();
		}

		protected override void TearDownOldElement(CarouselView oldElement)
		{
			Controller?.TearDown();
			base.TearDownOldElement(oldElement);
		}

		void UpdateIsSwipeEnabled()
		{
			if (Carousel == null)
				return;

			Controller.CollectionView.ScrollEnabled = Carousel.IsSwipeEnabled;
		}

		void UpdateIsBounceEnabled()
		{
			if (Carousel == null)
				return;

			Controller.CollectionView.Bounces = Carousel.IsBounceEnabled;
		}
	}
}
