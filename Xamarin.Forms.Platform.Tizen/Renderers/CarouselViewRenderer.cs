namespace Xamarin.Forms.Platform.Tizen
{
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView, Native.CarouselView>
	{
		public CarouselViewRenderer()
		{
			RegisterPropertyHandler(CarouselView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(CarouselView.IsBounceEnabledProperty, UpdateIsBounceEnabled);
			RegisterPropertyHandler(CarouselView.IsSwipeEnabledProperty, UpdateIsSwipeEnabled);
			RegisterPropertyHandler(CarouselView.PositionProperty, UpdatePosition);
		}

		protected override Native.CarouselView CreateNativeControl(ElmSharp.EvasObject parent)
		{
			return new Native.CarouselView(parent);
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return Element.ItemsLayout;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);
			Control.Scroll.Scrolled += OnScrollStart;
			Control.Scroll.PageScrolled += OnScrollStop;
		}

		protected override void OnItemSelectedFromUI(object sender, SelectedItemChangedEventArgs e)
		{
			Element.Position = e.SelectedItemIndex;
			Element.CurrentItem = e.SelectedItem;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Control.Scroll.Scrolled -= OnScrollStart;
					Control.Scroll.PageScrolled -= OnScrollStop;
				}
			}
			base.Dispose(disposing);
		}

		void OnScrollStart(object sender, System.EventArgs e)
		{
			if (!Element.IsDragging)
			{
				Element.SetIsDragging(true);
			}
		}

		void OnScrollStop(object sender, System.EventArgs e)
		{
			if (Element.IsDragging)
			{
				Element.SetIsDragging(false);
			}
		}

		void UpdatePosition(bool initialize)
		{
			if (initialize)
			{
				return;
			}
			if (Element.Position > -1 && Element.Position < Control.Adaptor.Count)
			{
				Control.Adaptor.RequestItemSelected(Element.Position);
				Element.CurrentItem = Control.Adaptor[Element.Position];
			}
		}

		void UpdateIsBounceEnabled()
		{
			if (Element.IsBounceEnabled)
			{
				if (Control.LayoutManager.IsHorizontal)
				{
					Control.Scroll.HorizontalBounce = true;
					Control.Scroll.VerticalBounce = false;
				}
				else
				{
					Control.Scroll.HorizontalBounce = false;
					Control.Scroll.VerticalBounce = true;
				}
			}
			else
			{
				Control.Scroll.HorizontalBounce = false;
				Control.Scroll.VerticalBounce = false;
			}
		}

		void UpdateIsSwipeEnabled()
		{
			if (Element.IsSwipeEnabled)
			{
				Control.Scroll.ScrollBlock = ElmSharp.ScrollBlock.None;
			}
			else
			{
				if (Control.LayoutManager.IsHorizontal)
				{
					Control.Scroll.ScrollBlock = ElmSharp.ScrollBlock.Horizontal;
				}
				else
				{
					Control.Scroll.ScrollBlock = ElmSharp.ScrollBlock.Vertical;
				}
			}
		}
	}
}
