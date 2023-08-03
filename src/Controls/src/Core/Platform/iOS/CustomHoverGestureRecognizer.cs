#nullable disable
using System;
using System.Runtime.Versioning;
using Foundation;
using ObjCRuntime;
using UIKit;
using PreserveAttribute = Microsoft.Maui.Controls.Internals.PreserveAttribute;

namespace Microsoft.Maui.Controls.Platform.iOS;

[SupportedOSPlatform("ios13.0")]
[SupportedOSPlatform("maccatalyst13.0")]
internal class CustomHoverGestureRecognizer : UIHoverGestureRecognizer
{
	public CustomHoverGestureRecognizer(Action<UIHoverGestureRecognizer> action) : base(action)
	{
		ShouldRecognizeSimultaneously = (_, __) =>
		{
			return true;
		};
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
