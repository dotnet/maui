using System;
using System.Collections.Generic;
using System.ComponentModel;
using ElmSharp;
using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView, Native.CarouselView>
	{
		List<View> _oldViews = new List<View>();
		SmartEvent _animationStart;

		public CarouselViewRenderer()
		{
			RegisterPropertyHandler(CarouselView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(CarouselView.IsBounceEnabledProperty, UpdateIsBounceEnabled);
			RegisterPropertyHandler(CarouselView.IsSwipeEnabledProperty, UpdateIsSwipeEnabled);
			RegisterPropertyHandler(CarouselView.PositionProperty, UpdatePositionFromElement);
			RegisterPropertyHandler(CarouselView.CurrentItemProperty, UpdateCurrentItemFromElement);
		}

		protected override Native.CarouselView CreateNativeControl(EvasObject parent)
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
			if (e.NewElement != null)
			{
				Control.Scrolled += OnScrolled;
				Control.Scroll.DragStart += OnDragStart;
				Control.Scroll.DragStop += OnDragStop;
				_animationStart = new SmartEvent(Control.Scroll, Control.Scroll.RealHandle, ThemeConstants.Scroller.Signals.StartScrollAnimation);
				_animationStart.On += OnScrollStart;
			}
			Device.BeginInvokeOnMainThread(() => {
				UpdatePositionFromElement(false);
				UpdateCurrentItemFromElement(false);
			});
		}

		protected override void UpdateHorizontalScrollBarVisibility()
		{
			var visibility = Element.HorizontalScrollBarVisibility;
			if (visibility == ScrollBarVisibility.Default)
				visibility = ScrollBarVisibility.Never;
			Control.HorizontalScrollBarVisiblePolicy = visibility.ToNative();
		}

		protected override void UpdateVerticalScrollBarVisibility()
		{
			var visibility = Element.VerticalScrollBarVisibility;
			if (visibility == ScrollBarVisibility.Default)
				visibility = ScrollBarVisibility.Never;
			Control.VerticalScrollBarVisiblePolicy = visibility.ToNative();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == nameof(Element.ItemsSource))
			{
				Element.Position = 0;
			}
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
				}
			}
			base.Dispose(disposing);
		}

		void OnDragStart(object sender, EventArgs e)
		{
			Element.SetIsDragging(true);
			Element.IsScrolling = true;
		}

		void OnDragStop(object sender, EventArgs e)
		{
			Element.SetIsDragging(false);
			Element.IsScrolling = false;
		}

		void OnScrollStart(object sender, EventArgs e)
		{
			Element.IsScrolling = true;
		}

		void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			var scrolledIndex = e.CenterItemIndex;
			Element.SetValueFromRenderer(CarouselView.PositionProperty, scrolledIndex);
			Element.SetValueFromRenderer(CarouselView.CurrentItemProperty, Control.Adaptor[scrolledIndex]);
			Control.Adaptor.RequestItemSelected(Control.Adaptor[scrolledIndex]);
			Element.IsScrolling = false;

			if (Control.Adaptor is ItemTemplateAdaptor adaptor)
			{
				var newViews = new List<View>();
				var carouselPosition = Element.Position;
				var previousPosition = carouselPosition - 1;
				var nextPosition = carouselPosition + 1;

				for (int i = e.FirstVisibleItemIndex; i <= e.LastVisibleItemIndex; i++)
				{
					var itemView = adaptor.GetTemplatedView(i);
					if (itemView == null)
					{
						continue;
					}

					if (i == carouselPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.CurrentItemVisualState);
					}
					else if (i == previousPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.PreviousItemVisualState);
					}
					else if (i == nextPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.NextItemVisualState);
					}
					else
					{
						VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
					}

					newViews.Add(itemView);

					if (!Element.VisibleViews.Contains(itemView))
					{
						Element.VisibleViews.Add(itemView);
					}
				}

				foreach (var itemView in _oldViews)
				{
					if (!newViews.Contains(itemView))
					{
						VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
						if (Element.VisibleViews.Contains(itemView))
						{
							Element.VisibleViews.Remove(itemView);
						}
					}
				}
				_oldViews = newViews;
			}
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
				Control.Scroll.ScrollBlock = ScrollBlock.None;
			}
			else
			{
				if (Control.LayoutManager.IsHorizontal)
				{
					Control.Scroll.ScrollBlock = ScrollBlock.Horizontal;
				}
				else
				{
					Control.Scroll.ScrollBlock = ScrollBlock.Vertical;
				}
			}
		}
	}
}
