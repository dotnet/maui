using System;
using System.ComponentModel;
using AppKit;
using RectangleF = CoreGraphics.CGRect;
using ObjCRuntime;
using Foundation;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	public class ScrollViewRenderer : NSScrollView, IVisualElementRenderer
	{
		EventTracker _events;
		VisualElementTracker _tracker;
		ScrollToRequestedEventArgs _requestedScroll;
		IVisualElementRenderer _contentRenderer;

		public ScrollViewRenderer() : base(RectangleF.Empty)
		{
			ContentView = new FlippedClipView();
			DrawsBackground = false;
			AutohidesScrollers = true;
			ContentView.PostsBoundsChangedNotifications = false;
			NSNotificationCenter.DefaultCenter.AddObserver(this, new Selector(nameof(UpdateScrollPosition)), BoundsChangedNotification, ContentView);
			UpdateOrientation();
		}

		ScrollView ScrollView => Element as ScrollView;

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
		}

		public NSView NativeView => this;

		public void SetElement(VisualElement element)
		{
			_requestedScroll = null;
			var oldElement = Element;
			Element = element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				((ScrollView)oldElement).ScrollToRequested -= OnScrollToRequested;
			}

			if (element != null)
			{
				element.PropertyChanged += OnElementPropertyChanged;
				((ScrollView)element).ScrollToRequested += OnScrollToRequested;
				if (_tracker == null)
				{
					PackContent();

					_events = new EventTracker(this);
					_events.LoadEvents(this);

					_tracker = new VisualElementTracker(this);
					_tracker.NativeControlUpdated += OnNativeControlUpdated;
				}

				UpdateContentSize();
				UpdateBackgroundColor();
				UpdateBackground();
				UpdateVerticalScrollBarVisibility();
				UpdateHorizontalScrollBarVisibility();

				UpdateOrientation();
				RaiseElementChanged(new VisualElementChangedEventArgs(oldElement, element));
			}

			ResetNativeNonScroll();
		}

		public void SetElementSize(Size size)
		{
			Microsoft.Maui.Controls.Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(Element,
				new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public NSViewController ViewController => null;

		public override void Layout()
		{
			base.Layout();
			LayoutSubviews();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_tracker == null)
					return;

				SetElement(null);

				_tracker.NativeControlUpdated -= OnNativeControlUpdated;
				_tracker.Dispose();
				_tracker = null;

				_events.Dispose();
				_events = null;

				ClearContentRenderer();

				//NSNotificationCenter.DefaultCenter.RemoveObserver(this, BoundsChangedNotification);
			}

			base.Dispose(disposing);
		}

		void RaiseElementChanged(VisualElementChangedEventArgs e)
		{
			ElementChanged?.Invoke(this, e);
		}

		void PackContent()
		{
			ClearContentRenderer();

			if (ScrollView.Children.Count == 0 || !(ScrollView.Children[0] is VisualElement))
				return;

			var content = (VisualElement)ScrollView.Children[0];
			if (Platform.GetRenderer(content) == null)
				Platform.SetRenderer(content, Platform.CreateRenderer(content));

			_contentRenderer = Platform.GetRenderer(content);
			(ContentView as FlippedClipView).ContentRenderer = _contentRenderer;
			DocumentView = _contentRenderer.NativeView;

			ContentView.PostsBoundsChangedNotifications = true;
		}

		void LayoutSubviews()
		{
			if (_requestedScroll != null && Superview != null)
			{
				var request = _requestedScroll;
				_requestedScroll = null;
				OnScrollToRequested(this, request);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ScrollView.ContentSizeProperty.PropertyName)
				UpdateContentSize();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
				UpdateVerticalScrollBarVisibility();
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
				UpdateHorizontalScrollBarVisibility();
			else if (e.PropertyName == ScrollView.OrientationProperty.PropertyName)
				UpdateOrientation();
		}

		private void UpdateOrientation()
		{
			if (ScrollView == null)
				return;

			switch (ScrollView.Orientation)
			{
				case ScrollOrientation.Both:
					{
						HasHorizontalScroller = true;
						HasVerticalScroller = true;
						break;
					}
				case ScrollOrientation.Horizontal:
					{
						HasVerticalScroller = false;
						HasHorizontalScroller = true;
						break;
					}
				case ScrollOrientation.Vertical:
					{
						HasHorizontalScroller = false;
						HasVerticalScroller = true;
						break;
					}
			}
		}

		void OnNativeControlUpdated(object sender, EventArgs eventArgs)
		{
			UpdateContentSize();
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			if (Superview == null)
			{
				_requestedScroll = e;
				return;
			}

			Point scrollPoint = (e.Mode == ScrollToMode.Position)
				? new Point(e.ScrollX, e.ScrollY)
				: ScrollView.GetScrollPositionForElement(e.Element as VisualElement, e.Position);

			ContentView.ScrollToPoint(scrollPoint.ToPointF());
			ScrollView.SendScrollFinished();
		}

		void UpdateBackgroundColor()
		{
			if (Element.BackgroundColor == Color.Default)
			{
				if (DrawsBackground)
					DrawsBackground = false;
				if (BackgroundColor != NSColor.Clear)
					BackgroundColor = NSColor.Clear;
			}
			else
			{
				DrawsBackground = true;
				BackgroundColor = Element.BackgroundColor.ToNSColor();
			}
		}

		void UpdateBackground()
		{
			if (NativeView == null)
				return;

			Brush background = Element.Background;

			NativeView.UpdateBackground(background);
		}

		void UpdateContentSize()
		{
			if (ContentView == null || ScrollView == null)
				return;

			ContentView.Frame = new RectangleF(ContentView.Frame.X, ContentView.Frame.Y, Frame.Width, Frame.Height);
			ResetNativeNonScroll();
		}

		void UpdateVerticalScrollBarVisibility()
		{
			var verticalScrollBarVisibility = ScrollView.VerticalScrollBarVisibility;

			HasVerticalScroller = (verticalScrollBarVisibility == ScrollBarVisibility.Always || verticalScrollBarVisibility == ScrollBarVisibility.Default);
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			HasHorizontalScroller = (ScrollView.HorizontalScrollBarVisibility == ScrollBarVisibility.Always);
		}

		private bool ResetNativeNonScroll()
		{
			if (ContentView == null || ScrollView == null || ScrollView.Content == null)
				return false;

			if (Math.Abs(ScrollView.ScrollY) < 0.001 && Math.Abs(ScrollView.ScrollX) < 0.001 && ScrollView.Content.Height >= ScrollView.Height)
			{
				ContentView.ScrollToPoint(new CoreGraphics.CGPoint(0, 0));
				return true;
			}

			return false;
		}

		[Export(nameof(UpdateScrollPosition))]
		void UpdateScrollPosition()
		{
			if (ScrollView == null)
				return;

			var height = ScrollView.Height;
			var contentHeightOverflow = ScrollView.ContentSize.Height - height;
			if (contentHeightOverflow >= 0)
			{
				if (height >= 0)
				{
					var location = ContentView.DocumentVisibleRect().Location;
					if (location.Y > -1)
						ScrollView.SetScrolledPosition(Math.Max(0, location.X), Math.Max(0, contentHeightOverflow - location.Y));
				}
			}
			else
				ResetNativeNonScroll();
		}

		void ClearContentRenderer()
		{
			if ((ContentView as FlippedClipView) != null)
				(ContentView as FlippedClipView).ContentRenderer = null;

			_contentRenderer?.NativeView?.RemoveFromSuperview();
			_contentRenderer?.Dispose();
			_contentRenderer = null;
		}
	}
}
