using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform.iOS
{
	public class MauiImageView : UIImageView
	{
		public MauiImageView()
		{
		}

		public MauiImageView(CGRect frame)
			: base(frame)
		{
		}

		public override void MovedToWindow() =>
			WindowChanged?.Invoke(this, EventArgs.Empty);

		public event EventHandler? WindowChanged;
	}
}