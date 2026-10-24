using System;
using System.Runtime.CompilerServices;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	internal sealed class TouchEventFallbackHandler
	{
		static readonly ConditionalWeakTable<AView, GestureHandler> GestureHandlers = new();
		WeakReference<GestureHandler>? _gestureHandler;
		long _downTime;

		internal static void RegisterGestureHandler(AView view, Func<MotionEvent, bool> handler)
		{
			GestureHandlers.Remove(view);
			GestureHandlers.Add(view, new GestureHandler(view, handler));
		}

		internal static void UnregisterGestureHandler(AView view) =>
			GestureHandlers.Remove(view);

		internal bool OnTouchEvent(AView source, MotionEvent? e)
		{
			if (e is null)
				return false;

			if (e.ActionMasked == MotionEventActions.Down)
			{
				_gestureHandler = null;
				_downTime = e.DownTime;
				ForwardTouchEvent(source, e);
			}
			else if (e.DownTime == _downTime &&
				_gestureHandler?.TryGetTarget(out var handler) == true &&
				handler.View.IsAlive() &&
				GestureHandlers.TryGetValue(handler.View, out var current) &&
				ReferenceEquals(handler, current))
			{
				// Keep the selected ancestor for the sequence, even when its
				// detector declines an individual MOVE or UP.
				if (e.ActionMasked is MotionEventActions.Up or MotionEventActions.Cancel)
					_gestureHandler = null;

				ForwardTouchEvent(source, e, handler);
			}
			else
			{
				_gestureHandler = null;
			}

			// An unhandled foreground view still blocks lower siblings. Ancestor
			// gestures are offered the event directly, never by redispatching children.
			return true;
		}

		void ForwardTouchEvent(AView source, MotionEvent e, GestureHandler? target = null)
		{
			if (target is null)
			{
				var ancestor = source.Parent as AView;
				while (ancestor is not null && !GestureHandlers.TryGetValue(ancestor, out _))
					ancestor = ancestor.Parent as AView;

				if (ancestor is null)
					return;
			}

			using var forwardedEvent = MotionEvent.Obtain(e);
			if (forwardedEvent is null)
			{
				_gestureHandler = null;
				return;
			}
			for (var child = source; child.Parent is AView parent; child = parent)
			{
				TransformToParent(child, parent, forwardedEvent);
				if (target is not null && parent == target.View)
				{
					target.Handle(forwardedEvent);
					return;
				}
				if (target is null &&
					GestureHandlers.TryGetValue(parent, out var handler) &&
					handler.Handle(forwardedEvent))
				{
					_gestureHandler = new WeakReference<GestureHandler>(handler);
					return;
				}
			}

			if (target is not null)
			{
				_gestureHandler = null;
				forwardedEvent.Action = MotionEventActions.Cancel;
				target.Handle(forwardedEvent);
			}
		}

		static void TransformToParent(AView child, AView parent, MotionEvent e)
		{
			var matrix = child.Matrix;
			if (matrix is not null && !matrix.IsIdentity)
				e.Transform(matrix);

			e.OffsetLocation(child.Left - parent.ScrollX, child.Top - parent.ScrollY);
		}

		sealed class GestureHandler
		{
			internal GestureHandler(AView view, Func<MotionEvent, bool> handle)
			{
				View = view;
				Handle = handle;
			}

			internal AView View { get; }
			internal Func<MotionEvent, bool> Handle { get; }
		}
	}
}
