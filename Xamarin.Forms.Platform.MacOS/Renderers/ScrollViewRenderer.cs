using System;
using System.ComponentModel;
using AppKit;
using RectangleF = CoreGraphics.CGRect;
using ObjCRuntime;
using Foundation;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Xamarin.Forms.Platform.MacOS
{
	public class ScrollViewRenderer : NSScrollView, IVisualElementRenderer
	{
		EventTracker _events;
		VisualElementTracker _tracker;
		ScrollToRequestedEventArgs _requestedScroll;
		IVisualElementRenderer _contentRenderer;

		public ScrollViewRenderer() : base(RectangleF.Empty)
		{
			DrawsBackground = false;
			ContentView.PostsBoundsChangedNotifications = true;
			NSNotificationCenter.DefaultCenter.AddObserver(this, new Selector(nameof(UpdateScrollPosition)),
				BoundsChangedNotification, ContentView);
			HasVerticalScroller = true;
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
				oldElement.PropertyChanged -= HandlePropertyChanged;
				((ScrollView)oldElement).ScrollToRequested -= OnScrollToRequested;
			}

			if (element != null)
			{
				element.PropertyChanged += HandlePropertyChanged;
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

				OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
			}
		}

		public void SetElementSize(Size size)
		{
			Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(Element,
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

		void OnElementChanged(VisualElementChangedEventArgs e)
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

			DocumentView = _contentRenderer.NativeView;
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

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ScrollView.ContentSizeProperty.PropertyName)
				UpdateContentSize();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		void HandleScrollAnimationEnded(object sender, EventArgs e)
		{
			ScrollView.SendScrollFinished();
		}

		void HandleScrolled(object sender, EventArgs e)
		{
			UpdateScrollPosition();
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
				? new Point(e.ScrollX, Element.Height - e.ScrollY)
				: ScrollView.GetScrollPositionForElement(e.Element as VisualElement, e.Position);

			(DocumentView as NSView)?.ScrollPoint(scrollPoint.ToPointF());

			ScrollView.SendScrollFinished();
		}

		void UpdateBackgroundColor()
		{
			BackgroundColor = Element.BackgroundColor.ToNSColor(Color.Transparent);
		}

		void UpdateContentSize()
		{
			if (_contentRenderer == null)
				return;
			var contentSize = ((ScrollView)Element).ContentSize.ToSizeF();
			if (!contentSize.IsEmpty)
				_contentRenderer.NativeView.Frame = new RectangleF(0, Element.Height - contentSize.Height, contentSize.Width,
					contentSize.Height);
		}

		[Export(nameof(UpdateScrollPosition))]
		void UpdateScrollPosition()
		{
			var convertedPoint = (DocumentView as NSView)?.ConvertPointFromView(ContentView.Bounds.Location, ContentView);
			if (convertedPoint.HasValue)
				ScrollView.SetScrolledPosition(Math.Max(0, convertedPoint.Value.X), Math.Max(0, convertedPoint.Value.Y));
		}

		void ClearContentRenderer()
		{
			_contentRenderer?.NativeView?.RemoveFromSuperview();
			_contentRenderer?.Dispose();
			_contentRenderer = null;
		}
	}
}