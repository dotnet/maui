using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public interface ILayoutManager
	{
		Size Measure(double widthConstraint, double heightConstraint);
		void ArrangeChildren(Rectangle childBounds);
	}
}
