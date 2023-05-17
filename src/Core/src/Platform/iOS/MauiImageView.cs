using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiImageView : UIImageView
	{
		readonly WeakReference<IImageHandler>? _handler;

		public MauiImageView(IImageHandler handler) => _handler = new(handler);

		[Obsolete("Use MauiImageView(IImageHandler handler) instead.")]
		public MauiImageView()
		{
		}

		[Obsolete("Use MauiImageView(IImageHandler handler) instead.")]
		public MauiImageView(CGRect frame)
			: base(frame)
		{
		}

		public override void MovedToWindow()
		{
			if (_handler is not null && _handler.TryGetTarget(out var handler))
			{
				handler.OnWindowChanged();
			}
		}

		[Obsolete("Use IImageHandler.OnWindowChanged() instead.")]
		public event EventHandler? WindowChanged
		{
			add { }
			remove { }
		}
	}
}