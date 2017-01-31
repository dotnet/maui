using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Animation;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AScrollView = Android.Widget.ScrollView;

namespace Xamarin.Forms.Platform.Android
{
	public class ScrollViewRenderer : AScrollView, IVisualElementRenderer
	{
		ScrollViewContainer _container;
		HorizontalScrollView _hScrollView;
		bool _isAttached;
		internal bool ShouldSkipOnTouch;
		bool _isBidirectional;
		ScrollView _view;

		public ScrollViewRenderer() : base(Forms.Context)
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
				((IScrollViewController)oldElement).ScrollToRequested -= OnScrollToRequested;
			}
			if (element != null)
			{
				OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

				if (_container == null)
				{
					Tracker = new VisualElementTracker(this);
					_container = new ScrollViewContainer(_view, Forms.Context);
				}

				_view.PropertyChanged += HandlePropertyChanged;
				Controller.ScrollToRequested += OnScrollToRequested;

				LoadContent();
				UpdateBackgroundColor();

				UpdateOrientation();

				element.SendViewInitialized(this);

				if (!string.IsNullOrEmpty(element.AutomationId))
					ContentDescription = element.AutomationId;
			}
		}

		public VisualElementTracker Tracker { get; private set; }

		public void UpdateLayout()
		{
			if (Tracker != null)
				Tracker.UpdateLayout();
		}

		public ViewGroup ViewGroup
		{
			get { return this; }
		}

		public override void Draw(Canvas canvas)
		{
			canvas.ClipRect(canvas.ClipBounds);

			base.Draw(canvas);
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
				float dY = LastY - ev.RawY;
				LastY = ev.RawY;
				LastX = ev.RawX;
				if (ev.Action == MotionEventActions.Move)
				{
					ScrollBy(0, (int)dY);
					foreach (AHorizontalScrollView child in this.GetChildrenOfType<AHorizontalScrollView>())
					{
						child.ScrollBy((int)dX, 0);
						break;
					}
				}
			}
			return base.OnTouchEvent(ev);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			SetElement(null);

			if (disposing)
			{
				Tracker.Dispose();
				Tracker = null;
				RemoveAllViews();
				_container.Dispose();
				_container = null;
			}
		}

		protected override void OnAttachedToWindow()
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
			base.OnLayout(changed, left, top, right, bottom);
			if (_view.Content != null && _hScrollView != null)
				_hScrollView.Layout(0, 0, right - left, Math.Max(bottom - top, (int)Context.ToPixels(_view.Content.Height)));
		}

		protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
		{
			base.OnScrollChanged(l, t, oldl, oldt);

			UpdateScrollPosition(Forms.Context.FromPixels(l), Forms.Context.FromPixels(t));
		}

		internal void UpdateScrollPosition(double x, double y)
		{
			if (_view != null)
			{
				if (_view.Orientation == ScrollOrientation.Both)
				{
					if (x == 0)
						x = Forms.Context.FromPixels(_hScrollView.ScrollX);

					if (y == 0)
						y = Forms.Context.FromPixels(ScrollY);
				}

				Controller.SetScrolledPosition(x, y);
			}
		}

		static int GetDistance(double start, double position, double v)
		{
			return (int)(start + (position - start) * v);
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Content")
				LoadContent();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == ScrollView.OrientationProperty.PropertyName)
				UpdateOrientation();
		}

		void LoadContent()
		{
			_container.ChildView = _view.Content;
		}

		async void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
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
				cycle++;

				if (cycle >= 10)
					break;
			}

			var x = (int)Forms.Context.ToPixels(e.ScrollX);
			var y = (int)Forms.Context.ToPixels(e.ScrollY);
			int currentX = _view.Orientation == ScrollOrientation.Horizontal || _view.Orientation == ScrollOrientation.Both ? _hScrollView.ScrollX : ScrollX;
			int currentY = _view.Orientation == ScrollOrientation.Vertical || _view.Orientation == ScrollOrientation.Both ? ScrollY : _hScrollView.ScrollY;
			if (e.Mode == ScrollToMode.Element)
			{
				Point itemPosition = Controller.GetScrollPositionForElement(e.Element as VisualElement, e.Position);

				x = (int)Forms.Context.ToPixels(itemPosition.X);
				y = (int)Forms.Context.ToPixels(itemPosition.Y);
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
			SetBackgroundColor(Element.BackgroundColor.ToAndroid(Color.Transparent));
		}

		void UpdateOrientation()
		{
			if (_view.Orientation == ScrollOrientation.Horizontal || _view.Orientation == ScrollOrientation.Both)
			{
				if (_hScrollView == null)
					_hScrollView = new AHorizontalScrollView(Context, this);

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
	}
}