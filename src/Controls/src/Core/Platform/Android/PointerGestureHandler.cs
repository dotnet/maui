#nullable disable
using System;
using System.Runtime.Versioning;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class PointerGestureHandler : Java.Lang.Object, AView.IOnHoverListener
	{
		// Tracks the last button pressed so we can use it for subsequent Move/Up/Cancel
		ButtonsMask? _activeButton;

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
					// Determine the button for this action. For Move/Up/Cancel prefer the active button, if any.
					ButtonsMask current = GetPressedButton(e);
					ButtonsMask effectiveButton = current;

					switch (e.Action)
					{
						case MotionEventActions.Down:
							// Primary button goes through Down/Up
							_activeButton = current;
							effectiveButton = current;
							if (!CheckButtonMask(pgr, effectiveButton))
								continue;
							pgr.SendPointerPressed(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, effectiveButton);
							break;
						case MotionEventActions.Move:
							// Keep reporting the button that initiated the press if one is active
							effectiveButton = _activeButton ?? current;
							if (!CheckButtonMask(pgr, effectiveButton))
								continue;
							pgr.SendPointerMoved(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, effectiveButton);
							break;
						case MotionEventActions.Up:
							// ACTION_UP does not carry ActionButton. Use the active one if available.
							effectiveButton = _activeButton ?? current;
							if (!CheckButtonMask(pgr, effectiveButton))
								continue;
							pgr.SendPointerReleased(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, effectiveButton);
							// Clear active button after release
							_activeButton = null;
							break;
						case MotionEventActions.Cancel:
							// Treat cancel similar to release for active button, then exit
							effectiveButton = _activeButton ?? current;
							if (!CheckButtonMask(pgr, effectiveButton))
								continue;
							pgr.SendPointerExited(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, effectiveButton);
							_activeButton = null;
							break;
					}
				}
			}

			return false;
		}

		ButtonsMask GetPressedButton(MotionEvent motionEvent)
		{
			if (motionEvent == null)
				return ButtonsMask.Primary;

			var action = motionEvent.Action;

			// For explicit button change events (mouse/pen), use ActionButton to determine which button changed
			if (OperatingSystem.IsAndroidVersionAtLeast(23) &&
				(action == MotionEventActions.ButtonPress || action == MotionEventActions.ButtonRelease))
			{
#pragma warning disable CA1416 // Validate platform compatibility
				var actionButton = motionEvent.ActionButton; // Which button changed for this event
				if ((actionButton & MotionEventButtonState.Secondary) == MotionEventButtonState.Secondary)
					return ButtonsMask.Secondary;
				if ((actionButton & MotionEventButtonState.Primary) == MotionEventButtonState.Primary)
					return ButtonsMask.Primary;
#pragma warning restore CA1416 // Validate platform compatibility
			}

			// Otherwise, infer from current ButtonState (covers Move/Down/Up and API < 23)
			var buttonState = motionEvent.ButtonState;

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
			// If no buttons specified (enum backing value is 0), default to Primary only
			if ((int)recognizer.Buttons == 0)
				return currentButton == ButtonsMask.Primary;

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