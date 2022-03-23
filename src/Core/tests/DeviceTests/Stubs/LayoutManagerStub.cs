using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class LayoutManagerStub : ILayoutManager
	{
		public Size ArrangeChildren(Rect bounds)
		{
			return bounds.Size;
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint - 1, heightConstraint - 1);
		}
	}
}
