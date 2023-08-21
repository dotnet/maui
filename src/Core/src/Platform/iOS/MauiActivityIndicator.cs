// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiActivityIndicator : UIActivityIndicatorView
	{
		IActivityIndicator? _virtualView;

		public MauiActivityIndicator(CGRect rect, IActivityIndicator? virtualView) : base(rect)
		{
			_virtualView = virtualView;
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			if (_virtualView?.IsRunning == true)
				StartAnimating();
			else
				StopAnimating();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (_virtualView?.IsRunning == true)
				StartAnimating();
			else
				StopAnimating();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			_virtualView = null;
		}
	}
}