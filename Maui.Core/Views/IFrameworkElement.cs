using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui
{
	public interface IFrameworkElement
	{
		bool IsEnabled { get; }
		Color BackgroundColor { get; }
		Rectangle Frame { get; }
		IViewRenderer Renderer { get; set; }
		IFrameworkElement Parent { get; }

		void Arrange(Rectangle bounds);
		SizeRequest Measure(double widthConstraint, double heightConstraint);

		SizeRequest DesiredSize { get; }
		bool IsMeasureValid { get; }
		bool IsArrangeValid { get; }

		void InvalidateMeasure();
		void InvalidateArrange();
	}
}
