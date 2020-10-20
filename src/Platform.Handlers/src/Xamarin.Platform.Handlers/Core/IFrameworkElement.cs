using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface IFrameworkElement
	{
		bool IsEnabled { get; }
		Color BackgroundColor { get; }
		Rectangle Frame { get; }
		IViewHandler? Handler { get; set; }
		IFrameworkElement? Parent { get; }

		void Arrange(Rectangle bounds);
		Size Measure(double widthConstraint, double heightConstraint);

		Size DesiredSize { get; }
		bool IsMeasureValid { get; }
		bool IsArrangeValid { get; }

		void InvalidateMeasure();
		void InvalidateArrange();

		double Width { get; }
		double Height { get; }
	}
}