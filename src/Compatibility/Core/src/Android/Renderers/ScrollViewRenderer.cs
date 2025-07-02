using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using Point = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ScrollViewRenderer : NestedScrollView, IVisualElementRenderer, IEffectControlProvider, IScrollView
	{
		ScrollViewContainer _container;
		HorizontalScrollView _hScrollView;
		ScrollBarVisibility _defaultHorizontalScrollVisibility = 0;
		ScrollBarVisibility _defaultVerticalScrollVisibility = 0;
		bool _isAttached;
		internal bool ShouldSkipOnTouch;
		bool _isBidirectional;
		ScrollView _view;
		int _previousBottom;
		bool _isEnabled;
		bool _disposed;
		LayoutDirection _prevLayoutDirection = LayoutDirection.Ltr;
		bool _checkedForRtlScroll = false;

		public ScrollViewRenderer(Context context) : base(
			new ContextThemeWrapper(context, Resource.Style.scrollViewTheme), null,
			Resource.Attribute.scrollViewStyle)
		{
		}

		protected IScrollViewController Controller
		{
			get { return (IScrollViewController)Element; }
		}

		internal float LastX { get; set; }

		internal float LastY { get; set; }

		public VisualElement Element
		{
			get { return _view; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;
		event EventHandler<PropertyChangedEventArgs> IVisualElementRenderer.ElementPropertyChanged
		{
			add { ElementPropertyChanged += value; }
			remove { ElementPropertyChanged -= value; }
		}

		public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size(40, 40));
		}

		public void SetElement(VisualElement element)
		{
			ScrollView oldElement = _view;
			_view = (ScrollView)element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= HandlePropertyChanged;
				oldElement.LayoutChanged -= HandleLayoutChanged;

				((IScrollViewController)oldElement).ScrollToRequested -= OnScrollToRequested;
			}
			if (element != null)
			{
				OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

				if (_container == null)
				{
					Tracker = new VisualElementTracker(this);
					_container = new ScrollViewContainer(_view, Context);
				}

				_view.PropertyChanged += HandlePropertyChanged;
				_view.LayoutChanged += HandleLayoutChanged;

				Controller.ScrollToRequested += OnScrollToRequested;

				LoadContent();
				UpdateBackgroundColor();
				UpdateBackground();
				UpdateOrientation();
				UpdateIsEnabled();
				UpdateHorizontalScrollBarVisibility();
				UpdateVerticalScrollBarVisibility();
				UpdateFlowDirection();

				element.SendViewInitialized(this);

				if (!string.IsNullOrEmpty(element.AutomationId))
					ContentDescription = element.AutomationId;
			}

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		void HandleLayoutChanged(object sender, EventArgs e)
		{
			UpdateLayout();
		}

		void UpdateFlowDirection()
		{
			if (Element is IVisualElementController controller)
			{
				var flowDirection = controller.EffectiveFlowDirection.IsLeftToRight()
					? LayoutDirection.Ltr
					: LayoutDirection.Rtl;

				if (_prevLayoutDirection != flowDirection && _hScrollView != null)
				{
					_prevLayoutDirection = flowDirection;
					_hScrollView.LayoutDirection = flowDirection;
				}
			}
		}

		public VisualElementTracker Tracker { get; private set; }

		public void UpdateLayout()
		{
			Tracker?.UpdateLayout();
		}

		AView IVisualElementRenderer.View => this;

		[PortHandler]
		public override void Draw(Canvas canvas)
		{
			try
			{
				canvas.ClipRect(canvas.ClipBounds);

				base.Draw(canvas);
			}
			catch (Java.Lang.NullPointerException)
			{
				// This will most likely never run since UpdateScrollBars is called 
				// when the scrollbars visibilities are updated but I left it here
				// just in case there's an edge case that causes an exception
				this.HandleScrollBarVisibilityChange();
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (Element.InputTransparent)
				return false;

			// set the start point for the bidirectional scroll; 
			// Down is swallowed by other controls, so we'll just sneak this in here without actually preventing
			// other controls from getting the event.			
			if (_isBidirectional && ev.Action == MotionEventActions.Down)
			{
				LastY = ev.RawY;
				LastX = ev.RawX;
			}

			return base.OnInterceptTouchEvent(ev);
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
			if (!_isEnabled)
				return false;

			if (ShouldSkipOnTouch)
			{
				ShouldSkipOnTouch = false;
				return false;
			}

			// The nested ScrollViews will allow us to scroll EITHER vertically OR horizontally in a single gesture.
			// This will allow us to also scroll diagonally.
			// We'll fall through to the base event so we still get the fling from the ScrollViews.
			// We have to do this in both ScrollViews, since a single gesture will be owned by one or the other, depending
			// on the initial direction of movement (i.e., horizontal/vertical).
			if (_isBidirectional && !Element.InputTransparent)
			{
				float dX = LastX - ev.RawX;

				LastY = ev.RawY;
				LastX = ev.RawX;
				if (ev.Action == MotionEventActions.Move)
				{
					foreach (AHorizontalScrollView child in this.GetChildrenOfType<AHorizontalScrollView>())
					{
						child.ScrollBy((int)dX, 0);
						break;
					}
					// Fall through to base.OnTouchEvent, it'll take care of the Y scrolling				
				}
			}

			return base.OnTouchEvent(ev);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				SetElement(null);
				Tracker?.Dispose();
				Tracker = null;
				RemoveAllViews();
				_container?.Dispose();
				_container = null;
			}

			base.Dispose(disposing);
		}

		public override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			_isAttached = true;
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();

			_isAttached = false;
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			// If the scroll view has changed size because of soft keyboard dismissal
			// (while WindowSoftInputModeAdjust is set to Resize), then we may need to request a 
			// layout of the ScrollViewContainer
			bool requestContainerLayout = bottom > _previousBottom;
			_previousBottom = bottom;

			_container?.Measure(MeasureSpecFactory.MakeMeasureSpec(right - left, MeasureSpecMode.Unspecified),
				MeasureSpecFactory.MakeMeasureSpec(bottom - top, MeasureSpecMode.Unspecified));
			base.OnLayout(changed, left, top, right, bottom);
			if (_view.Content != null && _hScrollView != null)
				_hScrollView.Layout(0, 0, right - left, Math.Max(bottom - top, (int)Context.ToPixels(_view.Content.Height)));
			else if (_view.Content != null && requestContainerLayout)
				_container?.RequestLayout();

			// if the target sdk >= 17 then setting the LayoutDirection on the scroll view natively takes care of the scroll
			if (!_checkedForRtlScroll && _hScrollView != null && Element is IVisualElementController controller && controller.EffectiveFlowDirection.IsRightToLeft())
			{
				Post(() => UpdateScrollPosition(_hScrollView.ScrollX, ScrollY));
			}

			_checkedForRtlScroll = true;
		}

		protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
		{
			_checkedForRtlScroll = true;
			base.OnScrollChanged(l, t, oldl, oldt);
			var context = Context;
			UpdateScrollPosition(context.FromPixels(l), context.FromPixels(t));
		}

		internal void UpdateScrollPosition(double x, double y)
		{
			if (_view != null)
			{
				if (_view.Orientation == ScrollOrientation.Both)
				{
					var context = Context;

					if (x == 0)
						x = context.FromPixels(_hScrollView.ScrollX);

					if (y == 0)
						y = context.FromPixels(ScrollY);
				}

				Controller.SetScrolledPosition(x, y);
			}
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				OnRegisterEffect(platformEffect);
		}

		void OnRegisterEffect(PlatformEffect effect)
		{
			effect.Container = this;
			effect.Control = this;
		}

		static int GetDistance(double start, double position, double v)
		{
			return (int)(start + (position - start) * v);
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);

			if (e.PropertyName == "Content")
				LoadContent();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == ScrollView.OrientationProperty.PropertyName)
				UpdateOrientation();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
				UpdateHorizontalScrollBarVisibility();
			else if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
				UpdateVerticalScrollBarVisibility();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		void UpdateIsEnabled()
		{
			if (Element == null)
			{
				return;
			}

			_isEnabled = Element.IsEnabled;
		}

		void LoadContent()
		{
			_container.ChildView = _view.Content;
		}

		async void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			_checkedForRtlScroll = true;

			if (!_isAttached)
			{
				return;
			}

			// 99.99% of the time simply queuing to the end of the execution queue should handle this case.
			// However it is possible to end a layout cycle and STILL be layout requested. We want to
			// back off until all are done, even if they trigger layout storms over and over. So we back off
			// for 10ms tops then move on.
			var cycle = 0;
			while (IsLayoutRequested)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(1));

				if (_disposed)
					return;

				cycle++;

				if (cycle >= 10)
					break;
			}

			var context = Context;
			var x = (int)context.ToPixels(e.ScrollX);
			var y = (int)context.ToPixels(e.ScrollY);
			int currentX = _view.Orientation == ScrollOrientation.Horizontal || _view.Orientation == ScrollOrientation.Both ? _hScrollView.ScrollX : ScrollX;
			int currentY = _view.Orientation == ScrollOrientation.Vertical || _view.Orientation == ScrollOrientation.Both ? ScrollY : _hScrollView.ScrollY;
			if (e.Mode == ScrollToMode.Element)
			{
				Point itemPosition = Controller.GetScrollPositionForElement(e.Element as VisualElement, e.Position);

				x = (int)context.ToPixels(itemPosition.X);
				y = (int)context.ToPixels(itemPosition.Y);
			}
			if (e.ShouldAnimate)
			{
				ValueAnimator animator = ValueAnimator.OfFloat(0f, 1f);
				animator.SetDuration(1000);
				animator.Update += (o, animatorUpdateEventArgs) =>
				{
					var v = (double)animatorUpdateEventArgs.Animation.AnimatedValue;
					int distX = GetDistance(currentX, x, v);
					int distY = GetDistance(currentY, y, v);

					if (_view == null)
					{
						// This is probably happening because the page with this Scroll View
						// was popped off the stack during animation
						animator.Cancel();
						return;
					}

					switch (_view.Orientation)
					{
						case ScrollOrientation.Horizontal:
							_hScrollView.ScrollTo(distX, distY);
							break;
						case ScrollOrientation.Vertical:
							ScrollTo(distX, distY);
							break;
						default:
							_hScrollView.ScrollTo(distX, distY);
							ScrollTo(distX, distY);
							break;
					}
				};
				animator.AnimationEnd += delegate
				{
					if (Controller == null)
						return;
					Controller.SendScrollFinished();
				};

				animator.Start();
			}
			else
			{
				switch (_view.Orientation)
				{
					case ScrollOrientation.Horizontal:
						_hScrollView.ScrollTo(x, y);
						break;
					case ScrollOrientation.Vertical:
						ScrollTo(x, y);
						break;
					default:
						_hScrollView.ScrollTo(x, y);
						ScrollTo(x, y);
						break;
				}
				Controller.SendScrollFinished();
			}
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
		}

		void UpdateBackgroundColor()
		{
			SetBackgroundColor(Element.BackgroundColor.ToAndroid(Colors.Transparent));
		}

		void UpdateBackground()
		{
			Brush background = Element.Background;

			this.UpdateBackground(background);
		}

		void UpdateOrientation()
		{
			if (_view.Orientation == ScrollOrientation.Horizontal || _view.Orientation == ScrollOrientation.Both)
			{
				if (_hScrollView == null)
				{
					_hScrollView = new AHorizontalScrollView(Context, this);
					_hScrollView.HorizontalFadingEdgeEnabled = HorizontalFadingEdgeEnabled;
					_hScrollView.SetFadingEdgeLength(HorizontalFadingEdgeLength);
					UpdateFlowDirection();
				}

				((AHorizontalScrollView)_hScrollView).IsBidirectional = _isBidirectional = _view.Orientation == ScrollOrientation.Both;

				if (_hScrollView.Parent != this)
				{
					_container.RemoveFromParent();
					_hScrollView.AddView(_container);
					AddView(_hScrollView);
				}
			}
			else
			{
				if (_container.Parent != this)
				{
					_container.RemoveFromParent();
					if (_hScrollView != null)
						_hScrollView.RemoveFromParent();
					AddView(_container);
				}
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_hScrollView != null)
			{
				if (_defaultHorizontalScrollVisibility == 0)
				{
					_defaultHorizontalScrollVisibility = _hScrollView.HorizontalScrollBarEnabled ? ScrollBarVisibility.Always : ScrollBarVisibility.Never;
				}

				var newHorizontalScrollVisiblility = _view.HorizontalScrollBarVisibility;

				if (newHorizontalScrollVisiblility == ScrollBarVisibility.Default)
				{
					newHorizontalScrollVisiblility = _defaultHorizontalScrollVisibility;
				}

				_hScrollView.HorizontalScrollBarEnabled = newHorizontalScrollVisiblility == ScrollBarVisibility.Always;
			}
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == 0)
				_defaultVerticalScrollVisibility = VerticalScrollBarEnabled ? ScrollBarVisibility.Always : ScrollBarVisibility.Never;

			var newVerticalScrollVisibility = _view.VerticalScrollBarVisibility;

			if (newVerticalScrollVisibility == ScrollBarVisibility.Default)
				newVerticalScrollVisibility = _defaultVerticalScrollVisibility;

			VerticalScrollBarEnabled = newVerticalScrollVisibility == ScrollBarVisibility.Always;

			this.HandleScrollBarVisibilityChange();
		}

		void IScrollView.AwakenScrollBars()
		{
			base.AwakenScrollBars();
		}

		bool IScrollView.ScrollBarsInitialized { get; set; } = false;
	}
}
