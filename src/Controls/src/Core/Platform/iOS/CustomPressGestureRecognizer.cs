#nullable disable
using System;
using Foundation;
using ObjCRuntime;
using UIKit;
using Microsoft.Maui.Controls;
using PreserveAttribute = Microsoft.Maui.Controls.Internals.PreserveAttribute;

namespace Microsoft.Maui.Controls.Platform.iOS;

internal class CustomPressGestureRecognizer : UIGestureRecognizer
{
	NSObject _target;
	UIEvent _currentEvent;
	ButtonsMask _detectedButton = ButtonsMask.Primary;

	public CustomPressGestureRecognizer(NSObject target, Selector action) : base(target, action)
	{
		_target = target;
		CancelsTouchesInView = false;
	}

	public CustomPressGestureRecognizer(Action<UIGestureRecognizer> action)
		: this(new Callback(action), Selector.FromHandle(Selector.GetHandle("target:"))!) { }

	/// <summary>
	/// Gets the detected button mask for the current gesture
	/// </summary>
	public ButtonsMask DetectedButton => _detectedButton;

	/// <summary>
	/// Gets the current UIEvent associated with this gesture
	/// </summary>
	public UIEvent CurrentEvent => _currentEvent;

	[Register("__UIGestureRecognizer")]
	class Callback : Token
	{
		Action<UIGestureRecognizer> action;

		internal Callback(Action<UIGestureRecognizer> action)
		{
			this.action = action;
		}

		[Export("target:")]
		[Preserve(Conditional = true)]
		public void Activated(UIGestureRecognizer sender)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13))
				action(sender);
		}
	}

	public override void TouchesBegan(NSSet touches, UIEvent evt)
	{
		_currentEvent = evt;
		_detectedButton = DetermineButtonFromEvent(evt, touches);
		State = UIGestureRecognizerState.Began;
		base.TouchesBegan(touches, evt);
	}

	public override void TouchesEnded(NSSet touches, UIEvent evt)
	{
		_currentEvent = evt;
		// Keep the same button that was detected during TouchesBegan
		State = UIGestureRecognizerState.Ended;
		base.TouchesEnded(touches, evt);
	}

	public override void TouchesMoved(NSSet touches, UIEvent evt)
	{
		_currentEvent = evt;
		State = UIGestureRecognizerState.Changed;
		base.TouchesMoved(touches, evt);
	}

	public override void TouchesCancelled(NSSet touches, UIEvent evt)
	{
		_currentEvent = evt;
		State = UIGestureRecognizerState.Cancelled;
		base.TouchesCancelled(touches, evt);
	}

	ButtonsMask DetermineButtonFromEvent(UIEvent evt, NSSet touches)
	{
		if (evt == null)
			return ButtonsMask.Primary;

		if (OperatingSystem.IsIOSVersionAtLeast(13, 4))
		{
			if (evt.Type == UIEventType.Presses && evt.ButtonMask != UIEventButtonMask.Primary)
			{
				if ((evt.ButtonMask & UIEventButtonMask.Secondary) == UIEventButtonMask.Secondary)
					return ButtonsMask.Secondary;
			}
		}

		if (OperatingSystem.IsMacCatalyst())
		{
			if (evt.ButtonMask != UIEventButtonMask.Primary)
			{
				if ((evt.ButtonMask & UIEventButtonMask.Secondary) == UIEventButtonMask.Secondary)
					return ButtonsMask.Secondary;
			}

			// Also check touch type for indirect input (mouse/trackpad)
			if (touches?.AnyObject is UITouch touch)
			{
				if (touch.Type == UITouchType.Indirect)
				{
					if ((evt.ButtonMask & UIEventButtonMask.Secondary) == UIEventButtonMask.Secondary)
						return ButtonsMask.Secondary;
				}
			}
		}

		return ButtonsMask.Primary;
	}
}
