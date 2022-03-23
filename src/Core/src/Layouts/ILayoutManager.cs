using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public interface ILayoutManager
	{
		Size Measure(double widthConstraint, double heightConstraint);

		Size ArrangeChildren(Rect bounds);
	}
}
