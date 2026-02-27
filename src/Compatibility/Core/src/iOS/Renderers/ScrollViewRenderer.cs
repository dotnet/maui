using System;
using System.ComponentModel;
using CoreGraphics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ScrollViewRenderer : UIScrollView, IVisualElementRenderer, IEffectControlProvider
	{
		EventTracker _events;
#pragma warning disable CS0618 // Type or member is obsolete
		KeyboardInsetTracker _insetTracker;
#pragma warning restore CS0618 // Type or member is obsolete

		VisualElementPackager _packager;

		RectangleF _previousFrame;
		ScrollToRequestedEventArgs _requestedScroll;
		VisualElementTracker _tracker;
		bool _checkedForRtlScroll = false;
		bool _previousLTR = true;

		[Preserve(Conditional = true)]
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


#pragma warning disable CS0618 // Type or member is obsolete
					_insetTracker = new KeyboardInsetTracker(this, () => Window, insets =>
					{
						ContentInset = ScrollIndicatorInsets = insets;
					},
					point =>
					{
						var offset = ContentOffset;
						offset.Y += point.Y;
						SetContentOffset(offset, true);
					}, this);
#pragma warning restore CS0618 // Type or member is obsolete
				}

				UpdateDelaysContentTouches();
				UpdateContentSize();
				UpdateBackgroundColor();
				UpdateBackground();
				UpdateIsEnabled();
				UpdateVerticalScrollBarVisibility();
				UpdateHorizontalScrollBarVisibility();

				OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

				EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

				element?.SendViewInitialized(this);

				if (!string.IsNullOrEmpty(element.AutomationId))
					AccessibilityIdentifier = element.AutomationId;
			}
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rect(Element.X, Element.Y, size.Width, size.Height));
		}

		public UIViewController ViewController
		{
			get { return null; }
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (Superview != null && ScrollView != null)
			{
				if (_requestedScroll != null)
				{
					var request = _requestedScroll;
					_requestedScroll = null;
					OnScrollToRequested(this, request);
				}
				else
				{
					UpdateBackground();
					UpdateFlowDirection();
				}
			}

			if (_previousFrame != Frame)
			{
				_previousFrame = Frame;
				_insetTracker?.UpdateInsets();
			}
		}

		void UpdateFlowDirection()
		{
			if (Superview == null || ScrollView.Content == null || _requestedScroll != null || _checkedForRtlScroll)
				return;

			if (Element is IVisualElementController controller && ScrollView.Orientation != ScrollOrientation.Vertical)
			{
				var isLTR = controller.EffectiveFlowDirection.IsLeftToRight();
				if (_previousLTR != isLTR)
				{
					_previousLTR = isLTR;
					_checkedForRtlScroll = true;
					SetContentOffset(new PointF((nfloat)(ScrollView.Content.Width - ScrollView.Width - ContentOffset.X), 0), false);
				}
			}

			_checkedForRtlScroll = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_packager == null)
					return;

				Element?.ClearValue(Platform.RendererProperty);
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

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e) => ElementChanged?.Invoke(this, e);

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty.PropertyName)
				UpdateDelaysContentTouches();
			else if (e.PropertyName == ScrollView.ContentSizeProperty.PropertyName)
				UpdateContentSize();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
				UpdateVerticalScrollBarVisibility();
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
				UpdateHorizontalScrollBarVisibility();
		}

		[PortHandler]
		void UpdateIsEnabled()
		{
			if (Element == null)
			{
				return;
			}

			ScrollEnabled = Element.IsEnabled;
		}

		void UpdateVerticalScrollBarVisibility()
		{
			var verticalScrollBarVisibility = ScrollView.VerticalScrollBarVisibility;
			ShowsVerticalScrollIndicator = verticalScrollBarVisibility == ScrollBarVisibility.Always
										   || verticalScrollBarVisibility == ScrollBarVisibility.Default;
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			var horizontalScrollBarVisibility = ScrollView.HorizontalScrollBarVisibility;
			ShowsHorizontalScrollIndicator = horizontalScrollBarVisibility == ScrollBarVisibility.Always
										   || horizontalScrollBarVisibility == ScrollBarVisibility.Default;
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
			var elementContentSize = RetrieveElementContentSize();
			ContentSize = elementContentSize.IsEmpty ? Bounds.Size : elementContentSize;
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			_checkedForRtlScroll = true;

			if (Superview == null)
			{
				_requestedScroll = e;
				return;
			}

			PointF newOffset = PointF.Empty;
			if (e.Mode == ScrollToMode.Position)
				newOffset = new PointF((nfloat)e.ScrollX, (nfloat)e.ScrollY);
			else
			{
				var positionOnScroll = ScrollView.GetScrollPositionForElement(e.Element as VisualElement, e.Position);
				positionOnScroll.X = positionOnScroll.X.Clamp(0, ContentSize.Width);
				positionOnScroll.Y = positionOnScroll.Y.Clamp(0, ContentSize.Height);

				switch (ScrollView.Orientation)
				{
					case ScrollOrientation.Horizontal:
						newOffset = new PointF((nfloat)positionOnScroll.X, ContentOffset.Y);
						break;
					case ScrollOrientation.Vertical:
						newOffset = new PointF(ContentOffset.X, (nfloat)positionOnScroll.Y);
						break;
					case ScrollOrientation.Both:
						newOffset = new PointF((nfloat)positionOnScroll.X, (nfloat)positionOnScroll.Y);
						break;
				}
			}
			var sameOffset = newOffset == ContentOffset;
			SetContentOffset(newOffset, e.ShouldAnimate);

			if (!e.ShouldAnimate || sameOffset)
				ScrollView.SendScrollFinished();
		}

		[PortHandler]
		void UpdateDelaysContentTouches()
		{
			DelaysContentTouches = ((ScrollView)Element).OnThisPlatform().ShouldDelayContentTouches();
		}

		void UpdateBackgroundColor()
		{
			BackgroundColor = Element.BackgroundColor.ToPlatform(Colors.Transparent);
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
			var contentSize = RetrieveElementContentSize();
			if (!contentSize.IsEmpty)
				ContentSize = contentSize;
		}

		CoreGraphics.CGSize RetrieveElementContentSize()
		{
			return ((ScrollView)Element).ContentSize.ToSizeF();
		}

		void UpdateScrollPosition()
		{
			ScrollView?.SetScrolledPosition(ContentOffset.X, ContentOffset.Y);
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, this, NativeView);
		}
	}
}
