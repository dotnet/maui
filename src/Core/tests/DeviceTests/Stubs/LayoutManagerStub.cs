using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class LayoutManagerStub : ILayoutManager
	{
		public Size ArrangeChildren(Rectangle childBounds)
		{
			return childBounds.Size;
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}
	}
}
