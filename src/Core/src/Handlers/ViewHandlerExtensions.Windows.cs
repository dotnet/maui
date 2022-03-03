using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static partial class ViewHandlerExtensions
	{
		internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView == null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			widthConstraint = AdjustForExplicitSize(widthConstraint, platformView.Width);
			heightConstraint = AdjustForExplicitSize(heightConstraint, platformView.Height);

			var measureConstraint = new global::Windows.Foundation.Size(widthConstraint, heightConstraint);

			platformView.Measure(measureConstraint);

			return new Size(platformView.DesiredSize.Width, platformView.DesiredSize.Height);
		}

		internal static void PlatformArrangeHandler(this IViewHandler viewHandler, Rect rect)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			platformView.Arrange(new global::Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));

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
