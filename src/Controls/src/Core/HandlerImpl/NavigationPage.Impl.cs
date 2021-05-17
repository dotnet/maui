using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage : IView
	{
		Thickness IView.Margin => Thickness.Zero;

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (Content is IFrameworkElement frameworkElement)
			{
				frameworkElement.Measure(widthConstraint, heightConstraint);
			}

			return new Size(widthConstraint, heightConstraint);
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			// Update the Bounds (Frame) for this page
			Layout(bounds);

			if (Content is IFrameworkElement element)
			{
				element.Arrange(bounds);
				element.Handler?.NativeArrange(element.Frame);
			}

			return Frame.Size;
		}

		IFrameworkElement Content =>
			this.CurrentPage;
	}

}
