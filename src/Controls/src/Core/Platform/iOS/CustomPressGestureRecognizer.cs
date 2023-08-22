#nullable disable
using System;
using Foundation;
using ObjCRuntime;
using UIKit;
using PreserveAttribute = Microsoft.Maui.Controls.Internals.PreserveAttribute;

namespace Microsoft.Maui.Controls.Platform.iOS;

internal class CustomPressGestureRecognizer : UIGestureRecognizer
{
	NSObject _target;

	public CustomPressGestureRecognizer(NSObject target, Selector action) : base(target, action)
	{
		_target = target;
	}

	public CustomPressGestureRecognizer(Action<UIGestureRecognizer> action)
		: this(new Callback(action), Selector.FromHandle(Selector.GetHandle("target:"))!) { }

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
		State = UIGestureRecognizerState.Began;
		base.TouchesBegan(touches, evt);
	}

	public override void TouchesEnded(NSSet touches, UIEvent evt)
	{
		State = UIGestureRecognizerState.Ended;
		base.TouchesEnded(touches, evt);
	}

	public override void TouchesMoved(NSSet touches, UIEvent evt)
	{
		State = UIGestureRecognizerState.Changed;
		base.TouchesMoved(touches, evt);
	}
}
