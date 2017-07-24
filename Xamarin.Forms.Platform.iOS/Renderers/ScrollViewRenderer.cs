using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	public class ScrollViewRenderer : UIScrollView, IVisualElementRenderer, IEffectControlProvider
	{
		EventTracker _events;
		KeyboardInsetTracker _insetTracker;

		VisualElementPackager _packager;

		RectangleF _previousFrame;
		ScrollToRequestedEventArgs _requestedScroll;
		VisualElementTracker _tracker;

		public ScrollViewRenderer() : base(RectangleF.Empty)
		{
			ScrollAnimationEnded += HandleScrollAnimationEnded;
			Scrolled += HandleScrolled;
		}

		ScrollView ScrollView
		{
			get { return Element as ScrollView; }
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
		}

		public UIView NativeView
		{
			get { return this; }
		}

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
				if (_packager == null)
				{
					_packager = new VisualElementPackager(this);
					_packager.Load();

					_tracker = new VisualElementTracker(this);
					_tracker.NativeControlUpdated += OnNativeControlUpdated;
					_events = new EventTracker(this);
					_events.LoadEvents(this);

					_insetTracker = new KeyboardInsetTracker(this, () => Window, insets => ContentInset = ScrollIndicatorInsets = insets, point =>
					{
						var offset = ContentOffset;
						offset.Y += point.Y;
						SetContentOffset(offset, true);
					});
				}

				UpdateDelaysContentTouches();
				UpdateContentSize();
				UpdateBackgroundColor();
				UpdateIsEnabled();

				OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

				EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

				if (element != null)
					element.SendViewInitialized(this);

				if (!string.IsNullOrEmpty(element.AutomationId))
					AccessibilityIdentifier = element.AutomationId;
			}
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public UIViewController ViewController
		{
			get { return null; }
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (_requestedScroll != null && Superview != null)
			{
				var request = _requestedScroll;
				_requestedScroll = null;
				OnScrollToRequested(this, request);
			}

			if (_previousFrame != Frame)
			{
				_previousFrame = Frame;
				_insetTracker?.UpdateInsets();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_packager == null)
					return;

				SetElement(null);

				_packager.Dispose();
				_packager = null;

				_tracker.NativeControlUpdated -= OnNativeControlUpdated;
				_tracker.Dispose();
				_tracker = null;

				_events.Dispose();
				_events = null;

				_insetTracker.Dispose();
				_insetTracker = null;

				ScrollAnimationEnded -= HandleScrollAnimationEnded;
				Scrolled -= HandleScrolled;
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty.PropertyName)
				UpdateDelaysContentTouches();
			else if (e.PropertyName == ScrollView.ContentSizeProperty.PropertyName)
				UpdateContentSize();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
		}

		void UpdateIsEnabled()
		{
			if (Element == null)
			{
				return;
			}

			ScrollEnabled = Element.IsEnabled;
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
			ContentSize = Bounds.Size;
			UpdateContentSize();
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			if (Superview == null)
			{
				_requestedScroll = e;
				return;
			}
			if (e.Mode == ScrollToMode.Position)
				SetContentOffset(new PointF((nfloat)e.ScrollX, (nfloat)e.ScrollY), e.ShouldAnimate);
			else
			{
				var positionOnScroll = ScrollView.GetScrollPositionForElement(e.Element as VisualElement, e.Position);

				positionOnScroll.X = positionOnScroll.X.Clamp(0, ContentSize.Width - Bounds.Size.Width);
				positionOnScroll.Y = positionOnScroll.Y.Clamp(0, ContentSize.Height - Bounds.Size.Height);

				switch (ScrollView.Orientation)
				{
					case ScrollOrientation.Horizontal:
						SetContentOffset(new PointF((nfloat)positionOnScroll.X, ContentOffset.Y), e.ShouldAnimate);
						break;
					case ScrollOrientation.Vertical:
						SetContentOffset(new PointF(ContentOffset.X, (nfloat)positionOnScroll.Y), e.ShouldAnimate);
						break;
					case ScrollOrientation.Both:
						SetContentOffset(new PointF((nfloat)positionOnScroll.X, (nfloat)positionOnScroll.Y), e.ShouldAnimate);
						break;
				}
			}
			if (!e.ShouldAnimate)
				ScrollView.SendScrollFinished();
		}

		void UpdateDelaysContentTouches()
		{
			DelaysContentTouches = ((ScrollView)Element).OnThisPlatform().ShouldDelayContentTouches();
		}

		void UpdateBackgroundColor()
		{
			BackgroundColor = Element.BackgroundColor.ToUIColor(Color.Transparent);
		}

		void UpdateContentSize()
		{
			var contentSize = ((ScrollView)Element).ContentSize.ToSizeF();
			if (!contentSize.IsEmpty)
				ContentSize = contentSize;
		}

		void UpdateScrollPosition()
		{
			if (ScrollView != null)
				ScrollView.SetScrolledPosition(ContentOffset.X, ContentOffset.Y);
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, this, NativeView);
		}
	}
}