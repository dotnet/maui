using System;
using Microsoft.Maui.Graphics;
using WSize = global::Windows.Foundation.Size;

namespace Microsoft.Maui
{
	internal static partial class ViewHandlerExtensions
	{

		// TODO: Possibly reconcile this code with LayoutPanel.ArrangeOverride
		// If you make changes here please review if those changes should also
		// apply to LayoutPanel.ArrangeOverride
		internal static WSize? LayoutVirtualView(
			this IPlatformViewHandler viewHandler,
			WSize availableSize)
		{
			var virtualView = viewHandler.VirtualView;
			var platformView = viewHandler.PlatformView;

			if (virtualView == null || platformView == null)
			{
				return null;
			}

			virtualView.Arrange(new Rect(0, 0, availableSize.Width, availableSize.Height));
			return availableSize;
		}

		// TODO: Possibly reconcile this code with LayoutPanel.MeasureOverride
		// If you make changes here please review if those changes should also
		// apply to LayoutPanel.MeasureOverride
		internal static WSize? MeasureVirtualView(
			this IPlatformViewHandler viewHandler,
			WSize availableSize)
		{

			var virtualView = viewHandler.VirtualView;
			var platformView = viewHandler.PlatformView;

			if (virtualView == null || platformView == null)
			{
				return null;
			}

			var width = availableSize.Width;
			var height = availableSize.Height;

			var crossPlatformSize = virtualView.Measure(width, height);

			width = crossPlatformSize.Width;
			height = crossPlatformSize.Height;

			return new WSize(width, height);
		}

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
