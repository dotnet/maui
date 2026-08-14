using System;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes;

public class PathSharedResourcesMemoryLeakTests : BaseTestFixture
{
	// NOTE: Path creation MUST be in a separate method.
	// If created inline in the test method, the JIT keeps local variables alive for the
	// entire method scope in debug mode — making the path uncollectable regardless of the fix.
	// This is the standard .NET pattern for writing reliable GC/leak unit tests.

	static WeakReference CreatePathWithGeometry(PathGeometry geometry)
	{
		var path = new Path { Data = geometry };
		return new WeakReference(path);
	}

	static WeakReference CreatePathWithTransform(ScaleTransform transform)
	{
		var path = new Path { RenderTransform = transform };
		return new WeakReference(path);
	}

	static WeakReference CreatePathWithBoth(PathGeometry geometry, ScaleTransform transform)
	{
		var path = new Path { Data = geometry, RenderTransform = transform };
		return new WeakReference(path);
	}

	[Fact]
	public void SharedPathGeometry_DoesNotKeepPathAlive()
	{
		var sharedGeometry = new PathGeometry
		{
			Figures = new PathFigureCollection
			{
				new PathFigure
				{
					StartPoint = new Point(0, 0),
					Segments = new PathSegmentCollection
					{
						new LineSegment { Point = new Point(100, 100) }
					}
				}
			}
		};

		var pathRef = CreatePathWithGeometry(sharedGeometry);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.False(pathRef.IsAlive,
			"Path was not collected. The shared PathGeometry is holding a strong reference to " +
			"Path via its PropertyChanged event handler. Use WeakGeometryChangedProxy to fix.");

		// Ensures sharedGeometry is a live GC root during the Collect calls above.
		GC.KeepAlive(sharedGeometry);
	}

	[Fact]
	public void SharedRenderTransform_DoesNotKeepPathAlive()
	{
		var sharedTransform = new ScaleTransform(1.5, 1.5) { CenterX = 50, CenterY = 50 };

		var pathRef = CreatePathWithTransform(sharedTransform);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.False(pathRef.IsAlive,
			"Path was not collected. The shared ScaleTransform is holding a strong reference to " +
			"Path via its PropertyChanged event handler. Use WeakNotifyPropertyChangedProxy to fix.");

		GC.KeepAlive(sharedTransform);
	}

	[Fact]
	public void SharedGeometryAndTransform_DoNotKeepPathAlive()
	{
		var sharedGeometry = new PathGeometry
		{
			Figures = new PathFigureCollection
			{
				new PathFigure
				{
					StartPoint = new Point(0, 0),
					Segments = new PathSegmentCollection
					{
						new LineSegment { Point = new Point(50, 50) }
					}
				}
			}
		};
		var sharedTransform = new ScaleTransform(2.0, 2.0) { CenterX = 25, CenterY = 25 };

		var pathRef = CreatePathWithBoth(sharedGeometry, sharedTransform);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.False(pathRef.IsAlive,
			"Path was not collected. Shared PathGeometry and/or ScaleTransform are holding " +
			"strong references to Path via event subscriptions. Use weak proxies to fix.");

		GC.KeepAlive(sharedGeometry);
		GC.KeepAlive(sharedTransform);
	}
}
