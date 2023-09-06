using System;
using Android.Views;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// A helper class to correctly intercept touches if a view is overlapping another view.
	/// </summary>
	/// 
	/// <remarks>
	/// Normally dispatchTouchEvent feeds the touch events to its children one at a time, top child
	/// first, (and only to the children in the hit-test area of the event) stopping as soon as one
	/// of them has handled the event.
	/// 
	/// But to be consistent across the platforms, we don't want this behavior; if an element is not
	/// input transparent we don't want an event to "pass through it" and be handled by an element
	/// "behind/under" it. We just want the processing to end after the first non-transparent child,
	/// regardless of whether the event has been handled.
	///
	/// This is only an issue for a couple of controls; the interactive controls (switch, button,
	/// slider, etc) already "handle" their touches and the events don't propagate to other child
	/// controls. But for image, label, and box that doesn't happen. We can't have those controls lie
	/// about their events being handled because then the events won't propagate to *parent* controls
	/// (e.g., a frame with a label in it would never get a tap gesture from the label). In other
	/// words, we *want* parent propagation, but *do not want* sibling propagation. So we need to
	/// short-circuit base.DispatchTouchEvent here, but still return "false".
	///
	/// Duplicating the logic of ViewGroup.dispatchTouchEvent and modifying it slightly for our
	/// purposes is a non-starter; the method is too complex and does a lot of micro-optimization.
	/// Instead, we provide a signalling mechanism for the controls which don't already "handle" touch
	/// events to tell us that they will be lying about handling their event; they then return "true"
	/// to short-circuit base.DispatchTouchEvent.
	///
	/// The container gets this message and after it gets the "handled" result from dispatchTouchEvent, 
	/// it then knows to ignore that result and return false/unhandled. This allows the event to
	/// propagate up the tree.
	/// </remarks>
	static class TouchEventInterceptor
	{
		/// <summary>
		/// Determine if the view needs to be wrapped in order to correctly handle
		/// touch interception for input transparency.
		/// </summary>
		/// <param name="view">The cross-platform view that is being processed.</param>
		/// <param name="handler">The view handler of the view.</param>
		/// <returns>
		/// Returns true if the view needs to be wrapped, false if the view
		/// already supports input transparency to9uch interception.
		/// </returns>
		public static bool NeedsContainer(IView view, IViewHandler handler)
		{
			// layouts already handle input transparency correctly
			if (view is ILayout)
				return false;

			// INTERACTIVE views (eg: buttons and entries) that are input
			// transparent need to be wrapped so we can intercept touches and
			// do a pass-through to the control below it.
			// 
			// However, for views that indicate that they can do this themselves,
			// we can skip the wrapper.
			// 
			// Most views are interactive, so this code works for most cases,
			// but we do need to have special cases for NON-interactive views.
			// Some examples are: labels and images. For these cases we have two
			// options:
			//  1. Wrap
			//      - Return true in the ViewHandler.NeedsContainer property for
			//        that particular handler.
			//      - This will add another level of nesting in the UI which is
			//        bad for perf, but may be simplest.
			//  2. Inherit and Implement
			//      - Create a MauiXxx type and inherit from the platform type.
			//      - Implement IInputTransparentCapable interface.
			//      - Override the OnTouchEvent method and ensure at least:
			//          public override bool OnTouchEvent(MotionEvent? e) =>
			//            base.OnTouchEvent(e) ||
			//            TouchEventInterceptor.OnTouchEvent(this, e);
			//      - This reduces UI nesting, but potentially adds new types to
			//        maintain.
			if (view.InputTransparent && handler.PlatformView is not IInputTransparentCapable)
				return true;

			return false;
		}
		/// <summary>
		/// This method should be called by any view that will "intercept" the touches.
		/// </summary>
		/// <param name="platformView">The view that is receiving the touch event.</param>
		/// <param name="e">The touch event.</param>
		/// <returns>Returns true if the touch was "handled", false if the view does not want to do anything.</returns>
		public static bool OnTouchEvent(View? platformView, MotionEvent? e)
		{
			if (platformView is null || !platformView.IsAlive())
				return false;

			if (e is null || e.Action == MotionEventActions.Cancel)
				return false;

			var touchIntercepting = platformView.Parent as ITouchInterceptingView;
			if (touchIntercepting is null || ShouldPassThroughElement(platformView))
				return false;

			// Let the container know that we are "fake" handling this event.
			touchIntercepting.TouchEventNotReallyHandled = true;
			return true;
		}

		/// <summary>
		/// This method should only be called before base.DispatchTouchEvent()
		/// and by the view that handles the interception logic.
		/// </summary>
		/// <param name="platformView">The view that is receiving the touch event.</param>
		/// <param name="e">The touch event.</param>
		public static bool DispatchingTouchEvent<T>(T platformView, MotionEvent? e)
			where T : View, ITouchInterceptingView
		{
			if (platformView is null || !platformView.IsAlive())
				return false;

			// If the view is NOT a layout but IS input transparent, then the event SHOULD pass through it
			if (platformView is not LayoutViewGroup && platformView is WrapperView w && w.InputTransparent)
				return false;

			platformView.TouchEventNotReallyHandled = false;

			// Always return true because this always happens.
			return true;
		}

		/// <summary>
		/// This method should only be called after base.DispatchTouchEvent()
		/// and by the view that handles the interception logic.
		/// </summary>
		public static bool DispatchedTouchEvent<T>(T platformView, MotionEvent? e, View.IOnTouchListener? touchListener)
			where T : View, ITouchInterceptingView
		{
			if (platformView is null || !platformView.IsAlive())
				return false;

			if (platformView.TouchEventNotReallyHandled)
			{
				// If the child control returned true from its touch event handler but signalled that it was a fake "true", then we
				// don't consider the event truly "handled" yet.
				//
				// Since a child control short-circuited the normal dispatchTouchEvent stuff, this layout never got the chance for
				// IOnTouchListener.OnTouch and the OnTouchEvent override to try handling the touches; we'll do that now.
				//
				// Any associated Touch Listeners are called from DispatchTouchEvents if all children of this view return false
				// So here we are simulating both calls that would have typically been called from inside DispatchTouchEvent
				// but were not called due to the fake "true".

				var result = touchListener?.OnTouch(platformView, e) ?? false;
				return result || platformView.OnTouchEvent(e);
			}

			return true;
		}

		static bool ShouldPassThroughElement(View platformView)
		{
			// Check if the view is a layout
			if (platformView is LayoutViewGroup lvg)
			{
				// If the layout is NOT input transparent, then the event should NOT pass through it
				if (!lvg.InputTransparent)
					return false;

				// If the event is being bubbled up from a child which is NOT input transparent,
				// we do NOT want it to be passed through (just up the tree)
				if (platformView is ITouchInterceptingView tiv && tiv.TouchEventNotReallyHandled)
					return false;

				// This event is NOT being bubbled up by a child layout that is NOT InputTransparent
				return true;
			}

			// Check if the view is a control/wrapper of a control
			if (platformView is IInputTransparentCapable itc)
			{
				// If the view is NOT a layout but IS input transparent, then the event SHOULD pass through it
				if (itc.InputTransparent)
					return true;
			}

			return false;
		}
	}

	/// <summary>
	/// This interface is implemented by various container views that are
	/// listening for taps on their children.
	/// </summary>
	interface ITouchInterceptingView
	{
		bool TouchEventNotReallyHandled { get; set; }
	}

	/// <summary>
	/// This interface is implemented by various views that with to indicate
	/// that they can correctly handle input transparency touch input and do not
	/// need to be wrapped.
	/// </summary>
	interface IInputTransparentCapable
	{
		bool InputTransparent { get; set; }
	}
}
