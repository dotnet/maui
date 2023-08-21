// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
	NSObject _target;

	public CustomHoverGestureRecognizer(NSObject target, Selector action) : base(target, action)
	{
		_target = target;
	}

	internal CustomHoverGestureRecognizer(Action<UIHoverGestureRecognizer> action)
		: this(new Callback(action), Selector.FromHandle(Selector.GetHandle("target:"))!) { }

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
