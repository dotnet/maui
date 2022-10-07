using System;
using Android.Views;
using Microsoft.Maui.Controls.Platform.Android;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class PointerGestureHandler
	{
		internal PointerGestureHandler(Func<View> getView, Func<AView> getControl)
		{
			GetView = getView;
			GetControl = getControl;
			SetupHandlerForPointer();
		}

		Func<View> GetView { get; }
		Func<AView> GetControl { get; }

		public void SetupHandlerForPointer()
		{
			var view = GetView();
			if (view == null)
				return;

			var control = GetControl();
			if (control == null)
				return;

			if (HasAnyPointerGestures())
				OnPointerAction(view, control);

			return;
		}

		private void OnPointerAction(View view, AView control)
		{
			control?.SetOnHoverListener(CreatePointerRecognizer((v, e) =>
			{
				foreach (var gesture in view.GetCompositeGestureRecognizers())
				{
					if (gesture is PointerGestureRecognizer)
					{
						var pgr = gesture as PointerGestureRecognizer;
						switch (e.Action)
						{
							case MotionEventActions.HoverEnter:
								pgr.SendPointerEntered(view, (relativeTo) => CalculatePosition(view, e));
								break;
							case MotionEventActions.HoverMove:
								pgr.SendPointerMoved(view, (relativeTo) => CalculatePosition(view, e));
								break;
							case MotionEventActions.HoverExit:
								pgr.SendPointerExited(view, (relativeTo) => CalculatePosition(view, e));
								break;
						}
					}
				}
				return false;
			}));
		}

		Point? CalculatePosition(IElement element, MotionEvent e)
		{
			var context = GetView()?.Handler?.MauiContext?.Context;

			if (context == null)
				return null;

			if (e == null)
				return null;

			if (element == null)
			{
				return new Point(context.FromPixels(e.RawX), context.FromPixels(e.RawY));
			}

			if (element == GetView())
			{
				return new Point(context.FromPixels(e.GetX()), context.FromPixels(e.GetY()));
			}

			if (element?.Handler?.PlatformView is AView aView)
			{
				var location = aView.GetLocationOnScreenPx();

				var x = e.RawX - location.X;
				var y = e.RawY - location.Y;

				return new Point(context.FromPixels(x), context.FromPixels(y));
			}

			return null;
		}

		CustomHoverGestureRecognizer CreatePointerRecognizer(Func<AView, MotionEvent, bool> onHover)
		{
			var result = new CustomHoverGestureRecognizer(onHover);
			return result;
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