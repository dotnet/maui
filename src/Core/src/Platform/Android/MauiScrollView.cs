using System;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Lights;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : NestedScrollView, IScrollBarView, NestedScrollView.IOnScrollChangeListener
	{
		View? _content;

		MauiHorizontalScrollView? _hScrollView;
		bool _isBidirectional;
		ScrollOrientation _scrollOrientation = ScrollOrientation.Vertical;
		ScrollBarVisibility _defaultHorizontalScrollVisibility;
		ScrollBarVisibility _defaultVerticalScrollVisibility;
		ScrollBarVisibility _horizontalScrollVisibility;

		internal float LastX { get; set; }
		internal float LastY { get; set; }

		internal bool ShouldSkipOnTouch;

		public MauiScrollView(Context context) : base(context)
		{
		}

		public MauiScrollView(Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
		{
		}

		public MauiScrollView(Context context, Android.Util.IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		protected MauiScrollView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public void SetHorizontalScrollBarVisibility(ScrollBarVisibility scrollBarVisibility)
		{
			_horizontalScrollVisibility = scrollBarVisibility;
			if (_hScrollView == null)
			{
				return;
			}

			if (_defaultHorizontalScrollVisibility == 0)
			{
				_defaultHorizontalScrollVisibility = _hScrollView.HorizontalScrollBarEnabled ? ScrollBarVisibility.Always : ScrollBarVisibility.Never;
			}

			if (scrollBarVisibility == ScrollBarVisibility.Default)
			{
				scrollBarVisibility = _defaultHorizontalScrollVisibility;
			}

			_hScrollView.HorizontalScrollBarEnabled = scrollBarVisibility == ScrollBarVisibility.Always;
		}

		public void SetVerticalScrollBarVisibility(ScrollBarVisibility scrollBarVisibility)
		{
			if (_defaultVerticalScrollVisibility == 0)
				_defaultVerticalScrollVisibility = VerticalScrollBarEnabled ? ScrollBarVisibility.Always : ScrollBarVisibility.Never;

			if (scrollBarVisibility == ScrollBarVisibility.Default)
				scrollBarVisibility = _defaultVerticalScrollVisibility;

			VerticalScrollBarEnabled = scrollBarVisibility == ScrollBarVisibility.Always;

			this.HandleScrollBarVisibilityChange();
		}

		public void SetContent(View content)
		{
			_content = content;
			SetOrientation(_scrollOrientation);
		}

		public void SetOrientation(ScrollOrientation orientation)
		{
			_scrollOrientation = orientation;

			if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both)
			{
				if (_hScrollView == null)
				{
					_hScrollView = new MauiHorizontalScrollView(Context, this);
					_hScrollView.HorizontalFadingEdgeEnabled = HorizontalFadingEdgeEnabled;
					_hScrollView.SetFadingEdgeLength(HorizontalFadingEdgeLength);
					SetHorizontalScrollBarVisibility(_horizontalScrollVisibility);
				}

				_hScrollView.IsBidirectional = _isBidirectional = orientation == ScrollOrientation.Both;

				if (_hScrollView.Parent != this)
				{
					if (_content != null)
					{
						_content.RemoveFromParent();
						_hScrollView.AddView(_content);
					}

					AddView(_hScrollView);
				}
			}
			else
			{
				if (_content != null && _content.Parent != this)
				{
					_content.RemoveFromParent();
					if (_hScrollView != null)
						_hScrollView.RemoveFromParent();
					AddView(_content);
				}
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent? ev)
		{
			// See also MauiHorizontalScrollView notes in OnInterceptTouchEvent

			if (ev == null)
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

		public override bool OnTouchEvent(MotionEvent? ev)
		{
			if (ev == null || !Enabled)
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
			if (_isBidirectional) // // See also MauiHorizontalScrollView notes in OnInterceptTouchEvent
			{
				float dX = LastX - ev.RawX;

				LastY = ev.RawY;
				LastX = ev.RawX;
				if (ev.Action == MotionEventActions.Move)
				{
					foreach (MauiHorizontalScrollView child in this.GetChildrenOfType<MauiHorizontalScrollView>())
					{
						child.ScrollBy((int)dX, 0);
						break;
					}
					// Fall through to base.OnTouchEvent, it'll take care of the Y scrolling				
				}
			}

			return base.OnTouchEvent(ev);
		}

		void IScrollBarView.AwakenScrollBars()
		{
			base.AwakenScrollBars();
		}

		bool IScrollBarView.ScrollBarsInitialized { get; set; }

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);

			if (_hScrollView?.Parent == this && _content is not null)
			{
				var scrollViewContentHeight = _content.Height;
				var hScrollViewHeight = bottom - top;
				var hScrollViewWidth = right - left;

				//if we are scrolling both ways we need to lay out our MauiHorizontalScrollView with more than the available height
				//so its parent the NestedScrollView can scroll vertically
				hScrollViewHeight = _isBidirectional ? Math.Max(hScrollViewHeight, scrollViewContentHeight) : hScrollViewHeight;

				// Ensure that the horizontal scrollview has been measured at the actual target size
				// so that it updates its internal bookkeeping to use the full size
				_hScrollView.Measure(MeasureSpec.MakeMeasureSpec(right - left, MeasureSpecMode.Exactly),
					MeasureSpec.MakeMeasureSpec(hScrollViewHeight, MeasureSpecMode.Exactly));

				_hScrollView.Layout(0, 0, hScrollViewWidth, hScrollViewHeight);
			}

			if (CrossPlatformArrange == null)
			{
				return;
			}

			var destination = Context!.ToCrossPlatformRectInReferenceFrame(left, top, right, bottom);

			CrossPlatformArrange(destination);
		}

		public void ScrollTo(int x, int y, bool instant, Action finished)
		{
			if (instant)
			{
				JumpTo(x, y, finished);
			}
			else
			{
				SmoothScrollTo(x, y, finished);
			}
		}

		void JumpTo(int x, int y, Action finished)
		{
			switch (_scrollOrientation)
			{
				case ScrollOrientation.Vertical:
					ScrollTo(x, y);
					break;
				case ScrollOrientation.Horizontal:
					_hScrollView?.ScrollTo(x, y);
					break;
				case ScrollOrientation.Both:
					_hScrollView?.ScrollTo(x, y);
					ScrollTo(x, y);
					break;
				case ScrollOrientation.Neither:
					break;
			}

			finished();
		}

		static int GetDistance(double start, double position, double v)
		{
			return (int)(start + (position - start) * v);
		}

		void SmoothScrollTo(int x, int y, Action finished)
		{
			int currentX = _scrollOrientation == ScrollOrientation.Horizontal || _scrollOrientation == ScrollOrientation.Both ? _hScrollView!.ScrollX : ScrollX;
			int currentY = _scrollOrientation == ScrollOrientation.Vertical || _scrollOrientation == ScrollOrientation.Both ? ScrollY : _hScrollView!.ScrollY;

			ValueAnimator? animator = ValueAnimator.OfFloat(0f, 1f);
			animator!.SetDuration(1000);

			animator.Update += (o, animatorUpdateEventArgs) =>
			{
				var v = (double)(animatorUpdateEventArgs.Animation!.AnimatedValue!);
				int distX = GetDistance(currentX, x, v);
				int distY = GetDistance(currentY, y, v);

				switch (_scrollOrientation)
				{
					case ScrollOrientation.Horizontal:
						_hScrollView?.ScrollTo(distX, distY);
						break;
					case ScrollOrientation.Vertical:
						ScrollTo(distX, distY);
						break;
					default:
						_hScrollView?.ScrollTo(distX, distY);
						ScrollTo(distX, distY);
						break;
				}
			};

			animator.AnimationEnd += delegate
			{
				finished();
			};

			animator.Start();
		}

#pragma warning disable CA1822 // DO NOT REMOVE! Needed because dotnet format will else try to make this static and break things
		void IOnScrollChangeListener.OnScrollChange(NestedScrollView v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
#pragma warning restore CA1822
		{
			OnScrollChanged(scrollX, scrollY, oldScrollX, oldScrollY);
		}

		internal Func<Graphics.Rect, Graphics.Size>? CrossPlatformArrange { get; set; }
	}

	internal class MauiHorizontalScrollView : HorizontalScrollView, IScrollBarView
	{
		readonly MauiScrollView? _parentScrollView;

		protected MauiHorizontalScrollView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public MauiHorizontalScrollView(Context? context, MauiScrollView parentScrollView) : base(context)
		{
			_parentScrollView = parentScrollView;
			Tag = "Microsoft.Maui.Android.HorizontalScrollView";
		}

		public MauiHorizontalScrollView(Context? context, IAttributeSet? attrs) : base(context, attrs)
		{
		}

		public MauiHorizontalScrollView(Context? context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		public MauiHorizontalScrollView(Context? context, IAttributeSet? attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
		}

		internal bool IsBidirectional { get; set; }

		public override void Draw(Canvas? canvas)
		{
			try
			{
				canvas?.ClipRect(canvas?.ClipBounds!);

				base.Draw(canvas!);
			}
			catch (Java.Lang.NullPointerException)
			{
				// This will most likely never run since UpdateScrollBars is called 
				// when the scrollbars visibilities are updated but I left it here
				// just in case there's an edge case that causes an exception
				this.HandleScrollBarVisibilityChange();
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent? ev)
		{
			if (ev == null || _parentScrollView == null)
				return false;

			// TODO ezhart 2021-07-12 The previous version of this checked _renderer.Element.InputTransparent; we don't have acces to that here,
			// and I'm not sure it even applies. We need to determine whether touch events will get here at all if we've marked the ScrollView InputTransparent
			// We _should_ be able to deal with it at the handler level by force-setting an OnTouchListener for the PlatformView that always returns false; then we
			// can just stop worrying about it here because the touches _can't_ reach this.

			// set the start point for the bidirectional scroll; 
			// Down is swallowed by other controls, so we'll just sneak this in here without actually preventing
			// other controls from getting the event.
			if (IsBidirectional && ev.Action == MotionEventActions.Down)
			{
				_parentScrollView.LastY = ev.RawY;
				_parentScrollView.LastX = ev.RawX;
			}

			return base.OnInterceptTouchEvent(ev);
		}

		public override bool OnTouchEvent(MotionEvent? ev)
		{
			if (ev == null || _parentScrollView == null)
				return false;

			if (!_parentScrollView.Enabled)
				return false;

			// If the touch is caught by the horizontal scrollview, forward it to the parent 
			_parentScrollView.ShouldSkipOnTouch = true;
			_parentScrollView.OnTouchEvent(ev);

			// The nested ScrollViews will allow us to scroll EITHER vertically OR horizontally in a single gesture.
			// This will allow us to also scroll diagonally.
			// We'll fall through to the base event so we still get the fling from the ScrollViews.
			// We have to do this in both ScrollViews, since a single gesture will be owned by one or the other, depending
			// on the initial direction of movement (i.e., horizontal/vertical).
			if (IsBidirectional)
			{
				float dY = _parentScrollView.LastY - ev.RawY;

				_parentScrollView.LastY = ev.RawY;
				_parentScrollView.LastX = ev.RawX;
				if (ev.Action == MotionEventActions.Move)
				{
					_parentScrollView.ScrollBy(0, (int)dY);
					// Fall through to base.OnTouchEvent, it'll take care of the X scrolling 					
				}
			}

			return base.OnTouchEvent(ev);
		}

		public override bool HorizontalScrollBarEnabled
		{
			get { return base.HorizontalScrollBarEnabled; }
			set
			{
				base.HorizontalScrollBarEnabled = value;
				this.HandleScrollBarVisibilityChange();
			}
		}

		void IScrollBarView.AwakenScrollBars()
		{
			base.AwakenScrollBars();
		}

		bool IScrollBarView.ScrollBarsInitialized { get; set; }

		protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
		{
			base.OnScrollChanged(l, t, oldl, oldt);

			if (_parentScrollView is NestedScrollView.IOnScrollChangeListener scrollChangeListener)
			{
				scrollChangeListener.OnScrollChange(_parentScrollView, l, t, oldl, oldt);
			}
		}
	}

	internal interface IScrollBarView
	{
		bool ScrollBarsInitialized { get; set; }
		bool ScrollbarFadingEnabled { get; set; }
		void AwakenScrollBars();
	}
}
