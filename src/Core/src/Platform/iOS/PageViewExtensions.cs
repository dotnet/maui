using System;
using System.Collections.Generic;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class PageViewExtensions
	{
		public static void UpdateBackground(this PageView nativeView, IView view)
		{
			var paint = view.Background;

			if (paint.IsNullOrEmpty())
				return;

			var color = paint.ToColor();

			if (color == null)
				return;

			nativeView.BackgroundColor = color.ToNative();
		}
	}
}
