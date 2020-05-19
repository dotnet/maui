using System;

namespace Xamarin.Forms.Platform.Tizen
{
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView, Native.CarouselView>
	{
		public CarouselViewRenderer()
		{
			RegisterPropertyHandler(CarouselView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(CarouselView.IsBounceEnabledProperty, UpdateIsBounceEnabled);
			RegisterPropertyHandler(CarouselView.IsSwipeEnabledProperty, UpdateIsSwipeEnabled);
			RegisterPropertyHandler(CarouselView.PositionProperty, UpdatePositionFromElement);
			RegisterPropertyHandler(CarouselView.CurrentItemProperty, UpdateCurrentItemFromElement);
		}

		protected override Native.CarouselView CreateNativeControl(ElmSharp.EvasObject parent)
		{
			return new Native.CarouselView(parent);
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return Element.ItemsLayout;
		}

		ElmSharp.SmartEvent _animationStart;
		ElmSharp.SmartEvent _animationStop;
		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				Control.Scrolled += OnScrolled;
				Control.Scroll.DragStart += OnDragStart;
				Control.Scroll.DragStop += OnDragStop;
				_animationStart = new ElmSharp.SmartEvent(Control.Scroll, Control.Scroll.RealHandle, "scroll,anim,start");
				_animationStart.On += OnScrollStart;
				_animationStop = new ElmSharp.SmartEvent(Control.Scroll, Control.Scroll.RealHandle, "scroll,anim,stop");
				_animationStop.On += OnScrollStop;
			}
			UpdatePositionFromElement(false);
			UpdateCurrentItemFromElement(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Control.Scrolled -= OnScrolled;
					Control.Scroll.DragStart -= OnDragStart;
					Control.Scroll.DragStop -= OnDragStop;
					_animationStart.On -= OnScrollStart;
					_animationStop.On -= OnScrollStop;
				}
			}
			base.Dispose(disposing);
		}

		void OnDragStart(object sender, System.EventArgs e)
		{
			Element.SetIsDragging(true);
			Element.IsScrolling = true;
		}

		void OnDragStop(object sender, System.EventArgs e)
		{
			Element.SetIsDragging(false);
			Element.IsScrolling = false;
		}

		void OnScrollStart(object sender, System.EventArgs e)
		{
			Element.IsScrolling = true;
		}

		void OnScrollStop(object sender, System.EventArgs e)
		{
			var scrollerIndex = Control.LayoutManager.IsHorizontal ? Control.Scroll.HorizontalPageIndex : Control.Scroll.VerticalPageIndex;
			Element.SetValueFromRenderer(CarouselView.PositionProperty, scrollerIndex);
			Element.SetValueFromRenderer(CarouselView.CurrentItemProperty, Control.Adaptor[scrollerIndex]);
			Control.Adaptor.RequestItemSelected(Control.Adaptor[scrollerIndex]);
			Element.IsScrolling = false;
		}

		void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			if (!Element.IsScrolling)
				Element.IsScrolling = true;

			if (Element.IsDragging)
				if (Element.Position != e.CenterItemIndex)
					Element.SetValueFromRenderer(CarouselView.PositionProperty, e.CenterItemIndex);
		}

		void UpdateCurrentItemFromElement(bool isInitializing)
		{
			if (isInitializing)
				return;

			if (Element.CurrentItem != null)
				ScrollTo(Control.Adaptor.GetItemIndex(Element.CurrentItem));
		}

		void UpdatePositionFromElement(bool isInitializing)
		{
			if (isInitializing)
				return;

			ScrollTo(Element.Position);
		}

		void ScrollTo(int position)
		{
			if (Element.IsScrolling)
				return;

			if (position > -1 && position < Control.Adaptor.Count)
			{
				var scrollerIndex = Control.LayoutManager.IsHorizontal ? Control.Scroll.HorizontalPageIndex : Control.Scroll.VerticalPageIndex;
				if (position != scrollerIndex)
					Control.ScrollTo(position);
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
