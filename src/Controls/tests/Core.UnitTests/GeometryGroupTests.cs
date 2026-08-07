using Microsoft.Maui.Controls.Shapes;
using Rect = Microsoft.Maui.Graphics.Rect;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class GeometryGroupTests : BaseTestFixture
{
	[Fact]
	public void ClearUnsubscribesPreviousChildrenFromPropertyChanged()
	{
		var group = new GeometryGroup();
		var oldChild = new RectangleGeometry(new Rect(0, 0, 10, 10));
		var newChild = new RectangleGeometry(new Rect(0, 0, 5, 5));
		var invalidations = 0;

		group.InvalidateGeometryRequested += (_, _) => invalidations++;

		group.Children.Add(oldChild);
		invalidations = 0;

		group.Children.Clear();
		invalidations = 0;

		// If Reset handling does not unsubscribe old items, this mutation incorrectly invalidates the group.
		oldChild.Rect = new Rect(1, 1, 11, 11);
		Assert.Equal(0, invalidations);

		group.Children.Add(newChild);
		invalidations = 0;

		newChild.Rect = new Rect(2, 2, 6, 6);
		Assert.Equal(1, invalidations);
	}
}