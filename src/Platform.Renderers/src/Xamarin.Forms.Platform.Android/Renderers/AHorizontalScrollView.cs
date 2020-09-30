using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class AHorizontalScrollView : HorizontalScrollView, IScrollView
	{
		readonly ScrollViewRenderer _renderer;

		public AHorizontalScrollView(Context context, ScrollViewRenderer renderer) : base(context)
		{
			_renderer = renderer;
		}

		internal bool IsBidirectional { get; set; }

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (_renderer.Element.InputTransparent)
				return false;
			// set the start point for the bidirectional scroll; 
			// Down is swallowed by other controls, so we'll just sneak this in here without actually preventing
			// other controls from getting the event.
			if (IsBidirectional && ev.Action == MotionEventActions.Down)
			{
				_renderer.LastY = ev.RawY;
				_renderer.LastX = ev.RawX;
			}

			return base.OnInterceptTouchEvent(ev);
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
			if (!_renderer.Element.IsEnabled)
				return false;
			// If the touch is caught by the horizontal scrollview, forward it to the parent so custom renderers can be notified of the touch.
			var verticalScrollViewerRenderer = Parent as ScrollViewRenderer;
			if (verticalScrollViewerRenderer != null)
			{
				verticalScrollViewerRenderer.ShouldSkipOnTouch = true;
				verticalScrollViewerRenderer.OnTouchEvent(ev);
			}

			// The nested ScrollViews will allow us to scroll EITHER vertically OR horizontally in a single gesture.
			// This will allow us to also scroll diagonally.
			// We'll fall through to the base event so we still get the fling from the ScrollViews.
			// We have to do this in both ScrollViews, since a single gesture will be owned by one or the other, depending
			// on the initial direction of movement (i.e., horizontal/vertical).
			if (IsBidirectional)
			{
				float dY = _renderer.LastY - ev.RawY;

				_renderer.LastY = ev.RawY;
				_renderer.LastX = ev.RawX;
				if (ev.Action == MotionEventActions.Move)
				{
					var parent = (NestedScrollView)Parent;
					parent.ScrollBy(0, (int)dY);
					// Fall through to base.OnTouchEvent, it'll take care of the X scrolling 					
				}
			}

			return base.OnTouchEvent(ev);
		}

		protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
		{
			base.OnScrollChanged(l, t, oldl, oldt);

			_renderer.UpdateScrollPosition(Context.FromPixels(l), Context.FromPixels(t));
		}

		public override void Draw(Canvas canvas)
		{
			try
			{
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

		public override bool HorizontalScrollBarEnabled
		{
			get { return base.HorizontalScrollBarEnabled; }
			set
			{
				base.HorizontalScrollBarEnabled = value;
				this.HandleScrollBarVisibilityChange();
			}
		}

		void IScrollView.AwakenScrollBars()
		{
			base.AwakenScrollBars();
		}

		bool IScrollView.ScrollBarsInitialized { get; set; } = false;
	}
}