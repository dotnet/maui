#nullable disable
using System;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class PointerGestureHandler : Java.Lang.Object, AView.IOnHoverListener, AView.IOnTouchListener
	{
		internal PointerGestureHandler(Func<View> getView, Func<AView> getControl)
		{
			GetView = getView;
			GetControl = getControl;
			SetupHandlerForPointer();
		}

		Func<View> GetView { get; }
		Func<AView> GetControl { get; }

		public bool OnHover(AView control, MotionEvent e)
		{
			var view = GetView();

			if (view == null)
				return false;
			
			var platformPointerArgs = new PlatformPointerEventArgs(control, e);

			foreach (var gesture in view.GetCompositeGestureRecognizers())
			{
				if (gesture is PointerGestureRecognizer)
				{
					var pgr = gesture as PointerGestureRecognizer;
					switch (e.Action)
					{
						case MotionEventActions.HoverEnter:
							pgr.SendPointerEntered(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
							break;
						case MotionEventActions.HoverMove:
							pgr.SendPointerMoved(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
							break;
						case MotionEventActions.HoverExit:
							pgr.SendPointerExited(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
							break;
					}
				}
			}

			return false;
		}

		public bool OnTouch(AView control, MotionEvent e)
		{
			var view = GetView();
			if (view == null)
				return false;

			var platformPointerArgs = new PlatformPointerEventArgs(control, e);

			foreach (var gesture in view.GetCompositeGestureRecognizers())
			{
				var pgr = gesture as PointerGestureRecognizer;
				switch (e.Action)
				{
					case MotionEventActions.Move:
						pgr.SendPointerMoved(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
						break;
					case MotionEventActions.Down:
						pgr.SendPointerPressed(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
						break;
					case MotionEventActions.Up:
						pgr.SendPointerReleased(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
						break;
				}
			}

			return true;
		}

		public void SetupHandlerForPointer()
		{
			var view = GetView();
			if (view == null)
				return;

			var control = GetControl();
			if (control == null)
				return;

			if (HasAnyPointerGestures())
			{
				control.SetOnHoverListener(this);
				control.SetOnTouchListener(this);
			}
			else
			{
				control.SetOnHoverListener(null);
				control.SetOnTouchListener(null);
			}

			return;
		}

		public bool HasAnyPointerGestures()
		{
			var gestures = GetView().GetCompositeGestureRecognizers();
			if (gestures == null || gestures.Count == 0)
				return false;

			foreach (var gesture in gestures)
				if (gesture is PointerGestureRecognizer)
					return true;

			return false;
		}
	}
}