using System;
using System.ComponentModel;
using ElmSharp;
using Xamarin.Forms.Platform.Tizen.Native;
using EContainer = ElmSharp.Container;
using ERect = ElmSharp.Rect;
using NBox = Xamarin.Forms.Platform.Tizen.Native.Box;
using NScroller = Xamarin.Forms.Platform.Tizen.Native.Scroller;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.ScrollView;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ScrollViewRenderer : ViewRenderer<ScrollView, NScroller>
	{
		EContainer _scrollCanvas;
		int _defaultVerticalStepSize;
		int _defaultHorizontalStepSize;

		EvasBox EvasFormsCanvas => _scrollCanvas as EvasBox;

		NBox Canvas => _scrollCanvas as NBox;

		public ScrollViewRenderer()
		{
			RegisterPropertyHandler("Content", FillContent);
			RegisterPropertyHandler(ScrollView.OrientationProperty, UpdateOrientation);
			RegisterPropertyHandler(ScrollView.VerticalScrollBarVisibilityProperty, UpdateVerticalScrollBarVisibility);
			RegisterPropertyHandler(ScrollView.HorizontalScrollBarVisibilityProperty, UpdateHorizontalScrollBarVisibility);
			RegisterPropertyHandler(Specific.VerticalScrollStepProperty, UpdateVerticalScrollStep);
			RegisterPropertyHandler(Specific.HorizontalScrollStepProperty, UpdateHorizontalScrollStep);
		}

		public override ERect GetNativeContentGeometry()
		{
			return Forms.UseFastLayout ? EvasFormsCanvas.Geometry : Canvas.Geometry;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
		{
			if (Control == null)
			{
				SetNativeControl(CreateNativeControl());
				Control.Scrolled += OnScrolled;

				if (Forms.UseFastLayout)
				{
					_scrollCanvas = new EvasBox(Control);
					EvasFormsCanvas.LayoutUpdated += OnContentLayoutUpdated;
				}
				else
				{
					_scrollCanvas = new NBox(Control);
					Canvas.LayoutUpdated += OnContentLayoutUpdated;
				}

				Control.SetContent(_scrollCanvas);
				_defaultVerticalStepSize = Control.VerticalStepSize;
				_defaultHorizontalStepSize = Control.HorizontalStepSize;
			}

			if (e.OldElement != null)
			{
				(e.OldElement as IScrollViewController).ScrollToRequested -= OnScrollRequested;
			}

			if (e.NewElement != null)
			{
				(e.NewElement as IScrollViewController).ScrollToRequested += OnScrollRequested;
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (ScrollView.ContentSizeProperty.PropertyName == e.PropertyName)
			{
				UpdateContentSize();
			}
			else
			{
				base.OnElementPropertyChanged(sender, e);
			}
		}

		protected virtual NScroller CreateNativeControl()
		{
			if (Device.Idiom == TargetIdiom.Watch)
			{
				return new Native.Watch.WatchScroller(Forms.NativeParent, Forms.CircleSurface);
			}
			else
			{
				return new NScroller(Forms.NativeParent);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != Element)
				{
					(Element as IScrollViewController).ScrollToRequested -= OnScrollRequested;
				}

				if (Control != null)
				{
					Control.Scrolled -= OnScrolled;
				}
				if (Canvas != null)
				{
					Canvas.LayoutUpdated -= OnContentLayoutUpdated;
				}
				if (EvasFormsCanvas != null)
				{
					EvasFormsCanvas.LayoutUpdated -= OnContentLayoutUpdated;
				}
			}

			base.Dispose(disposing);
		}

		void FillContent()
		{
			if (Forms.UseFastLayout)
			{
				EvasFormsCanvas.UnPackAll();
				if (Element.Content != null)
				{
					EvasFormsCanvas.PackEnd(Platform.GetOrCreateRenderer(Element.Content).NativeView);
					UpdateContentSize();
				}
			}
			else
			{
				Canvas.UnPackAll();
				if (Element.Content != null)
				{
					Canvas.PackEnd(Platform.GetOrCreateRenderer(Element.Content).NativeView);
					UpdateContentSize();
				}
			}
		}

		void OnContentLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			UpdateContentSize();
		}

		void UpdateOrientation()
		{
			switch (Element.Orientation)
			{
				case ScrollOrientation.Horizontal:
					Control.ScrollBlock = ScrollBlock.Vertical;
					Control.HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Auto;
					Control.VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible;
					break;
				case ScrollOrientation.Vertical:
					Control.ScrollBlock = ScrollBlock.Horizontal;
					Control.HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible;
					Control.VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Auto;
					break;
				default:
					Control.ScrollBlock = ScrollBlock.None;
					Control.HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Auto;
					Control.VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Auto;
					break;
			}
		}

		void UpdateContentSize()
		{
			_scrollCanvas.MinimumWidth = Forms.ConvertToScaledPixel(Element.ContentSize.Width + Element.Padding.HorizontalThickness);
			_scrollCanvas.MinimumHeight = Forms.ConvertToScaledPixel(Element.ContentSize.Height + Element.Padding.VerticalThickness);

			// elm-scroller updates the CurrentRegion after render
			Device.BeginInvokeOnMainThread(() =>
			{
				if (Control != null)
				{
					OnScrolled(Control, EventArgs.Empty);
				}
			});
		}


		protected void OnScrolled(object sender, EventArgs e)
		{
			var region = Control.CurrentRegion.ToDP();
			((IScrollViewController)Element).SetScrolledPosition(region.X, region.Y);
		}

		async void OnScrollRequested(object sender, ScrollToRequestedEventArgs e)
		{
			var x = e.ScrollX;
			var y = e.ScrollY;
			if (e.Mode == ScrollToMode.Element)
			{
				Point itemPosition = (Element as IScrollViewController).GetScrollPositionForElement(e.Element as VisualElement, e.Position);
				x = itemPosition.X;
				y = itemPosition.Y;
			}

			ERect region = new Rectangle(x, y, Element.Width, Element.Height).ToPixel();
			await Control.ScrollToAsync(region, e.ShouldAnimate);
			Element.SendScrollFinished();
		}

		void UpdateVerticalScrollBarVisibility()
		{
			Control.VerticalScrollBarVisiblePolicy = Element.VerticalScrollBarVisibility.ToNative();
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			var orientation = Element.Orientation;
			if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both)
				Control.HorizontalScrollBarVisiblePolicy = Element.HorizontalScrollBarVisibility.ToNative();
		}

		void UpdateVerticalScrollStep(bool initialize)
		{
			var step = Specific.GetVerticalScrollStep(Element);
			if (initialize && step == -1)
				return;

			Control.VerticalStepSize = step != -1 ? Forms.ConvertToScaledPixel(step) : _defaultVerticalStepSize;
		}

		void UpdateHorizontalScrollStep(bool initialize)
		{
			var step = Specific.GetHorizontalScrollStep(Element);
			if (initialize && step == -1)
				return;

			Control.HorizontalStepSize = step != -1 ? Forms.ConvertToScaledPixel(step) : _defaultHorizontalStepSize;
		}
	}

	static class ScrollBarExtensions
	{
		public static ScrollBarVisiblePolicy ToNative(this ScrollBarVisibility visibility)
		{
			switch (visibility)
			{
				case ScrollBarVisibility.Default:
					return ScrollBarVisiblePolicy.Auto;
				case ScrollBarVisibility.Always:
					return ScrollBarVisiblePolicy.Visible;
				case ScrollBarVisibility.Never:
					return ScrollBarVisiblePolicy.Invisible;
				default:
					return ScrollBarVisiblePolicy.Auto;
			}
		}
	}
}
