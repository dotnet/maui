using Microsoft.Maui;

namespace Microsoft.Maui.Layouts
{
	public interface ILayoutManager
	{
		Size Measure(double widthConstraint, double heightConstraint);
		void ArrangeChildren(Rectangle childBounds);
	}
}
