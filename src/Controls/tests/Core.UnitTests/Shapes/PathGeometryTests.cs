using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes;

public class PathGeometryTests : BaseTestFixture
{
	/// <summary>
	/// Figures.Clear() must unsubscribe the cleared PathFigure from the PathGeometry,
	/// otherwise the figure retains the geometry alive via its PropertyChanged delegate.
	/// </summary>
	[Fact]
	public void FiguresClear_UnsubscribesFigurePropertyChangedHandler()
	{
		var geometry = new PathGeometry();
		var sharedFigure = new PathFigure { StartPoint = new Point(0, 0) };
		geometry.Figures.Add(sharedFigure);

		int invalidateCount = 0;
		geometry.InvalidatePathGeometryRequested += (s, e) => invalidateCount++;

		// Sanity-check: mutating the figure before Clear should trigger invalidation.
		sharedFigure.StartPoint = new Point(10, 10);
		Assert.Equal(1, invalidateCount);

		// Act - Clear() fires CollectionChanged (Reset), which itself calls Invalidate() once.
		geometry.Figures.Clear();
		int countAfterClear = invalidateCount;

		// After Clear, mutating the figure must NOT trigger any further invalidation on the geometry.
		sharedFigure.StartPoint = new Point(20, 20);
		Assert.Equal(countAfterClear, invalidateCount);
	}

	/// <summary>
	/// Figures.Clear() must unsubscribe the cleared PathFigure's segment-invalidation event
	/// from the PathGeometry.
	/// </summary>
	[Fact]
	public void FiguresClear_UnsubscribesFigureSegmentInvalidateHandler()
	{
		var geometry = new PathGeometry();
		var sharedFigure = new PathFigure { StartPoint = new Point(0, 0) };
		geometry.Figures.Add(sharedFigure);

		int invalidateCount = 0;
		geometry.InvalidatePathGeometryRequested += (s, e) => invalidateCount++;

		// Sanity-check: adding a segment before Clear should trigger invalidation.
		sharedFigure.Segments.Add(new LineSegment { Point = new Point(100, 100) });
		Assert.Equal(1, invalidateCount);

		// Act - Clear() fires CollectionChanged (Reset), which itself calls Invalidate() once.
		geometry.Figures.Clear();
		int countAfterClear = invalidateCount;

		// After Clear, adding segments to the cleared figure must NOT trigger any further invalidation.
		sharedFigure.Segments.Add(new LineSegment { Point = new Point(200, 200) });
		Assert.Equal(countAfterClear, invalidateCount);
	}

	/// <summary>
	/// After Figures.Clear(), the PathGeometry must be eligible for garbage collection
	/// even when the cleared PathFigure is still alive (shared/rooted elsewhere).
	/// </summary>
	[Fact]
	public void FiguresClear_AllowsPathGeometryToBeGarbageCollected()
	{
		var sharedFigure = new PathFigure { StartPoint = new Point(0, 0) };
		var weakRef = CreateGeometryAndClear(sharedFigure);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		// If the bug is present, sharedFigure still holds the geometry alive via
		// its PropertyChanged delegate chain, so TryGetTarget would return true.
		Assert.False(weakRef.TryGetTarget(out _),
			"PathGeometry was retained by the cleared PathFigure (event-handler leak in Figures.Clear()).");
	}

	// Factored out so the JIT cannot inline the PathGeometry local onto the caller's frame.
	[MethodImpl(MethodImplOptions.NoInlining)]
	static WeakReference<PathGeometry> CreateGeometryAndClear(PathFigure figure)
	{
		var geometry = new PathGeometry();
		geometry.Figures.Add(figure);
		geometry.Figures.Clear();
		return new WeakReference<PathGeometry>(geometry);
	}
}
