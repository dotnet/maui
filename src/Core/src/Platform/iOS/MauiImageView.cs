using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiImageView : UIImageView
	{
		WeakReference<ImageHandler>? _handler;

		public MauiImageView(ImageHandler handler) => _handler = new WeakReference<ImageHandler>(handler);

		[Obsolete("To be removed in a future release")]
		public MauiImageView()
		{
		}

		[Obsolete("To be removed in a future release")]
		public MauiImageView(CGRect frame)
			: base(frame)
		{
		}

		public override void MovedToWindow()
		{
			if (_handler is not null && _handler.TryGetTarget(out var handler))
			{
				handler.NotifyWindowChanged();
			}
		}

		[Obsolete("To be removed in a future release")]
		public event EventHandler? WindowChanged
		{
			add { }
			remove { }
		}
	}
}