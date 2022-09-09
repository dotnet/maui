using System;
using Foundation;
using ObjCRuntime;
using UIKit;
using PreserveAttribute = Microsoft.Maui.Controls.Internals.PreserveAttribute;

namespace Microsoft.Maui.Controls.Platform.iOS;

internal class CustomHoverGestureRecognizer : UIHoverGestureRecognizer
{
#pragma warning disable CA1416
	internal CustomHoverGestureRecognizer(Action<UIHoverGestureRecognizer> action)
		: base(new Callback(action), Selector.FromHandle(Selector.GetHandle("target:"))!) { }
#pragma warning restore CA1416

	[Register("__UIHoverGestureRecognizer")]
	class Callback : Token
	{
		Action<UIHoverGestureRecognizer> action;

		internal Callback(Action<UIHoverGestureRecognizer> action)
		{
			this.action = action;
		}

		[Export("target:")]
		[Preserve(Conditional = true)]
		public void Activated(UIHoverGestureRecognizer sender)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13))
				action(sender);
		}
	}
}
