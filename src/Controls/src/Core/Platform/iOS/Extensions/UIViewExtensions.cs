using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
#if __MOBILE__
using UIKit;
namespace Microsoft.Maui.Controls.Platform
#else
using UIView = AppKit.NSView;
namespace Microsoft.Maui.Controls.Platform
#endif
{
	public static class UIViewExtensions
	{
#if __MOBILE__
		public static UIImage ConvertToImage(this UIView view)
		{
			if (!NativeVersion.IsAtLeast(10))
			{
				UIGraphics.BeginImageContext(view.Frame.Size);
				view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
				var image = UIGraphics.GetImageFromCurrentImageContext();
				UIGraphics.EndImageContext();
				return new UIImage(image.CGImage);
			}

			var imageRenderer = new UIGraphicsImageRenderer(view.Bounds.Size);

			return imageRenderer.CreateImage((a) =>
			{
				view.Layer.RenderInContext(a.CGContext);
			});
		}
#endif
	}
}