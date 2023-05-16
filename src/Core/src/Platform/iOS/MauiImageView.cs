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

		[Obsolete("Use MauiImageView(ImageHandler handler) instead.")]
		public MauiImageView()
		{
		}

		[Obsolete("Use MauiImageView(ImageHandler handler) instead.")]
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

		[Obsolete("No longer fired on iOS, as it introduces a memory leak.")]
		public event EventHandler? WindowChanged
		{
			add { }
			remove { }
		}
	}
}