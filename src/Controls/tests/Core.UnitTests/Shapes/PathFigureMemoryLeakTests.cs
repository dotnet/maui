using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes;

public class PathFigureMemoryLeakTests : BaseTestFixture
{
	[Fact]
	public async Task SharedSegmentsCollectionDoesNotRetainPathFigure()
	{
		var sharedSegments = new PathSegmentCollection();
		var weakFigure = CreatePathFigure(sharedSegments);

		Assert.False(await weakFigure.WaitForCollect(), "PathFigure should not be alive!");
		GC.KeepAlive(sharedSegments);
	}

	static WeakReference CreatePathFigure(PathSegmentCollection segments)
	{
		var figure = new PathFigure { Segments = segments };
		return new WeakReference(figure);
	}
}
