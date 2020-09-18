using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface IFrameworkElement
	{
		bool IsEnabled { get; }
		Color BackgroundColor { get; }
		Rectangle Frame { get; }
		IViewHandler Handler { get; set; }
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