using System;
using System.ComponentModel;
using ElmSharp;
using NScroller = Xamarin.Forms.Platform.Tizen.Native.Scroller;
using NBox = Xamarin.Forms.Platform.Tizen.Native.Box;
using ERect = ElmSharp.Rect;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// This class provides a Renderer for a ScrollView widget.
	/// </summary>
	public class ScrollViewRenderer : ViewRenderer<ScrollView, NScroller>
	{
		NBox _scrollCanvas;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.ScrollViewRenderer"/> class.
		/// </summary>
		public ScrollViewRenderer()
		{
			RegisterPropertyHandler("Content", FillContent);
		}

		/// <summary>
		/// Provide Native cotnent area to place child content
		/// </summary>
		/// <returns>Rect of Content area</returns>
		public override ERect GetNativeContentGeometry()
		{
			return _scrollCanvas.Geometry;
		}

		/// <summary>
		/// Handles the element change event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new NScroller(Forms.NativeParent));
				Control.Scrolled += OnScrolled;
				_scrollCanvas = new NBox(Control);
				_scrollCanvas.LayoutUpdated += OnContentLayoutUpdated;
				Control.SetContent(_scrollCanvas);
			}

			if (e.OldElement != null)
			{
				(e.OldElement as IScrollViewController).ScrollToRequested -= OnScrollRequested;
			}

			if (e.NewElement != null)
			{
				(e.NewElement as IScrollViewController).ScrollToRequested += OnScrollRequested;
			}

			UpdateAll();

			base.OnElementChanged(e);
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
				if (_scrollCanvas != null)
				{
					_scrollCanvas.LayoutUpdated -= OnContentLayoutUpdated;
				}
			}

			base.Dispose(disposing);
		}

		void FillContent()
		{
			_scrollCanvas.UnPackAll();
			if (Element.Content != null)
			{
				_scrollCanvas.PackEnd(Platform.GetOrCreateRenderer(Element.Content).NativeView);
				UpdateContentSize();
			}

		}

		void OnContentLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			UpdateContentSize();
		}

		void UpdateAll()
		{
			UpdateOrientation();
			UpdateVerticalScrollBarVisibility();
			UpdateHorizontalScrollBarVisibility();
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
			_scrollCanvas.MinimumWidth = Forms.ConvertToScaledPixel(Element.ContentSize.Width);
			_scrollCanvas.MinimumHeight = Forms.ConvertToScaledPixel(Element.ContentSize.Height);

			// elm-scroller updates the CurrentRegion after render
			Device.BeginInvokeOnMainThread(() =>
			{
				if (Control != null)
				{
					OnScrolled(Control, EventArgs.Empty);
				}
			});
		}

		/// <summary>
		/// An event raised on element's property change.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments</param>
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (ScrollView.OrientationProperty.PropertyName == e.PropertyName)
			{
				UpdateOrientation();
			}
			else if (ScrollView.ContentSizeProperty.PropertyName == e.PropertyName)
			{
				UpdateContentSize();
			}
			else if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
				UpdateVerticalScrollBarVisibility();
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
				UpdateHorizontalScrollBarVisibility();

			base.OnElementPropertyChanged(sender, e);
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

			Rect region = new Rectangle(x, y, Element.Width, Element.Height).ToPixel();
			await Control.ScrollToAsync(region, e.ShouldAnimate);
			Element.SendScrollFinished();
		}

		void UpdateVerticalScrollBarVisibility()
		{
			Control.VerticalScrollBarVisiblePolicy = ScrollBarVisibilityToTizen(Element.VerticalScrollBarVisibility);
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			var orientation = Element.Orientation;
			if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both)
				Control.HorizontalScrollBarVisiblePolicy = ScrollBarVisibilityToTizen(Element.HorizontalScrollBarVisibility);
		}

		ScrollBarVisiblePolicy ScrollBarVisibilityToTizen(ScrollBarVisibility visibility)
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
