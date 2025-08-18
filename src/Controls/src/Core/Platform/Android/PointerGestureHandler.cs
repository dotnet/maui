#nullable disable
using System;
using Android.Views;
using Microsoft.Maui.Graphics;
using System.Runtime.Versioning;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class PointerGestureHandler : Java.Lang.Object, AView.IOnHoverListener
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

		// This method is called by InnerGestureListener to handle touch events for pointer gestures
		public bool OnTouch(MotionEvent e)
		{
			var view = GetView();

			if (view == null)
				return false;

			var control = GetControl();
			if (control == null)
				return false;

			var platformPointerArgs = new PlatformPointerEventArgs(control, e);

			foreach (var gesture in view.GetCompositeGestureRecognizers())
			{
				if (gesture is PointerGestureRecognizer pgr)
				{
					var pressedButton = GetPressedButton(e);
					
					// Check if this gesture should respond to the current button
					if (!CheckButtonMask(pgr, pressedButton))
						continue;

					switch (e.Action)
					{
						case MotionEventActions.Down:
							pgr.SendPointerPressed(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, pressedButton);
							break;
						case MotionEventActions.Move:
							pgr.SendPointerMoved(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, pressedButton);
							break;
						case MotionEventActions.Up:
							pgr.SendPointerReleased(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, pressedButton);
							break;
						case MotionEventActions.Cancel:
							pgr.SendPointerExited(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, pressedButton);
							break;
					}

					// Handle button press/release events on API 23+ ONLY for actual button events
					if (OperatingSystem.IsAndroidVersionAtLeast(23))
					{
#pragma warning disable CA1416 // Validate platform compatibility
						HandleButtonEvents(e, pgr, view, platformPointerArgs, pressedButton);
#pragma warning restore CA1416 // Validate platform compatibility
					}
				}
			}

			return false;
		}

		[SupportedOSPlatform("android23.0")]
		void HandleButtonEvents(MotionEvent e, PointerGestureRecognizer pgr, View view, PlatformPointerEventArgs platformPointerArgs, ButtonsMask pressedButton)
		{
			// Only handle explicit button press/release events, not regular touch events
			// This prevents duplicate events when a regular touch is also processed above
			switch (e.Action)
			{
				case MotionEventActions.ButtonPress:
					pgr.SendPointerPressed(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, pressedButton);
					break;
				case MotionEventActions.ButtonRelease:
					pgr.SendPointerReleased(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, pressedButton);
					break;
				// Do not handle Down/Up/Move/Cancel here as they are already handled above
			}
		}

		ButtonsMask GetPressedButton(MotionEvent motionEvent)
		{
			var buttonState = motionEvent?.ButtonState ?? MotionEventButtonState.Primary;

			// Check for secondary button (right mouse button)
			if ((buttonState & MotionEventButtonState.Secondary) == MotionEventButtonState.Secondary)
			{
				return ButtonsMask.Secondary;
			}

			// Check for stylus secondary button on API 23+
			if (OperatingSystem.IsAndroidVersionAtLeast(23))
			{
#pragma warning disable CA1416 // Validate platform compatibility
				if (CheckStylusSecondaryButton(buttonState))
#pragma warning restore CA1416 // Validate platform compatibility
				{
					return ButtonsMask.Secondary;
				}
			}

			// Default to primary button
			return ButtonsMask.Primary;
		}

		[SupportedOSPlatform("android23.0")]
		bool CheckStylusSecondaryButton(MotionEventButtonState buttonState)
		{
			return (buttonState & MotionEventButtonState.StylusSecondary) == MotionEventButtonState.StylusSecondary;
		}

		bool CheckButtonMask(PointerGestureRecognizer recognizer, ButtonsMask currentButton)
		{
			if (currentButton == ButtonsMask.Secondary)
			{
				return (recognizer.Buttons & ButtonsMask.Secondary) == ButtonsMask.Secondary;
			}

			return (recognizer.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary;
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
				control.SetOnHoverListener(this);
			else
				control.SetOnHoverListener(null);

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