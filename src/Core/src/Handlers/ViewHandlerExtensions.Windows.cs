using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static partial class ViewHandlerExtensions
	{
		internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
		{
			var nativeView = viewHandler.ToPlatform();

			if (nativeView == null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			widthConstraint = AdjustForExplicitSize(widthConstraint, nativeView.Width);
			heightConstraint = AdjustForExplicitSize(heightConstraint, nativeView.Height);

			var measureConstraint = new global::Windows.Foundation.Size(widthConstraint, heightConstraint);

			nativeView.Measure(measureConstraint);

			return new Size(nativeView.DesiredSize.Width, nativeView.DesiredSize.Height);
		}

		internal static void PlatformArrangeHandler(this IViewHandler viewHandler, Rectangle rect)
		{
			var nativeView = viewHandler.ToPlatform();

			if (nativeView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			nativeView.Arrange(new global::Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));

			viewHandler.Invoke(nameof(IView.Frame), rect);
		}

		static double AdjustForExplicitSize(double externalConstraint, double explicitValue)
		{
			// Even with an explicit value specified, Windows will limit the size of the control to 
			// the size of the parent's explicit size. Since we want our controls to get their
			// explicit sizes regardless (and possibly exceed the size of their layouts), we need
			// to measure them at _at least_ their explicit size.

			if (double.IsNaN(explicitValue))
			{
				// NaN for an explicit height/width on Windows means "unspecified", so we just use the external value
				return externalConstraint;
			}

			// If the control's explicit height/width is larger than the containers, use the control's value
			return Math.Max(externalConstraint, explicitValue);
		}
	}
}
