#if WINDOWS
using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Graphics.DeviceTests;

public class PathTests
{
	[Fact]
	public void GeometryGroupWithMoveOnlyFigureAndLineConverts()
	{
		var group = new GeometryGroup();
		group.Children.Add(new PathGeometry(new PathFigureCollection { new PathFigure() }));
		group.Children.Add(new LineGeometry(new Point(15, 15), new Point(19, 19)));
		var path = ((IShape)group).PathForBounds(new Rect(0, 0, 100, 100));
		var device = CanvasDevice.GetSharedDevice();

		foreach (var fillMode in new[] { CanvasFilledRegionDetermination.Winding, CanvasFilledRegionDetermination.Alternate })
		{
			using var platformGeometry = global::Microsoft.Maui.Graphics.Platform.GraphicsExtensions.AsCanvasGeometry(path, device, fillMode);
			using var win2DGeometry = global::Microsoft.Maui.Graphics.Win2D.W2DExtensions.AsPath(path, device, fillMode);

			Assert.NotNull(platformGeometry);
			Assert.NotNull(win2DGeometry);
			Assert.Equal(MathF.Sqrt(32), platformGeometry.ComputePathLength(), 3);
			Assert.Equal(MathF.Sqrt(32), win2DGeometry.ComputePathLength(), 3);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void MultipleOpenFiguresRemainIndependent(bool useWin2D)
	{
		var path = new PathF();
		path.MoveTo(10, 10);
		path.LineTo(100, 10);
		path.MoveTo(20, 20);
		path.LineTo(100, 20);

		using var geometry = ConvertPath(path, useWin2D);

		AssertGeometry(geometry, 170, 10, 10, 90, 10);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void MultipleClosedFiguresRemainIndependent(bool useWin2D)
	{
		var path = new PathF();
		path.MoveTo(10, 10);
		path.LineTo(100, 10);
		path.LineTo(100, 100);
		path.Close();
		path.MoveTo(150, 150);
		path.LineTo(200, 150);
		path.LineTo(200, 200);
		path.Close();

		using var geometry = ConvertPath(path, useWin2D);

		AssertGeometry(geometry, 280 + MathF.Sqrt(16200) + MathF.Sqrt(5000), 10, 10, 190, 190);
		Assert.InRange(MathF.Abs(geometry.ComputeArea() - 5300), 0f, 0.1f);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ArcFigureCanBeClosed(bool useWin2D)
	{
		var path = new PathF();
		path.MoveTo(10, 10);
		path.AddArc(10, 10, 100, 100, 0, 180, true);
		path.Close();

		using var geometry = ConvertPath(path, useWin2D);

		// Include the line to the arc start and the closing line back to (10, 10).
		AssertGeometry(geometry, MathF.Sqrt(10125) + 45 * MathF.PI + 45, 10, 10, 90, 90);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ArcAfterClosedFigureStartsIndependentFigure(bool useWin2D)
	{
		var path = new PathF();
		path.MoveTo(0, 0);
		path.LineTo(10, 0);
		path.LineTo(10, 10);
		path.Close();
		path.AddArc(100, 100, 120, 120, 0, 90, false);

		Assert.Equal(new[]
		{
			PathOperation.Move, PathOperation.Line, PathOperation.Line,
			PathOperation.Close, PathOperation.Arc
		}, path.SegmentTypes);
		Assert.Equal(2, path.SubPathCount);
		Assert.True(path.IsSubPathClosed(0));
		Assert.False(path.IsSubPathClosed(1));

		using var geometry = ConvertPath(path, useWin2D);

		// Only the triangle perimeter and quarter-circle arc contribute to the length.
		AssertGeometry(geometry, 20 + MathF.Sqrt(200) + 5 * MathF.PI, 0, 0, 120, 110);
	}

	static CanvasGeometry ConvertPath(PathF path, bool useWin2D)
	{
		var device = CanvasDevice.GetSharedDevice();
		return useWin2D
			? global::Microsoft.Maui.Graphics.Win2D.W2DExtensions.AsPath(path, device)
			: global::Microsoft.Maui.Graphics.Platform.GraphicsExtensions.AsCanvasGeometry(path, device);
	}

	static void AssertGeometry(CanvasGeometry geometry, float length, double x, double y, double width, double height)
	{
		Assert.NotNull(geometry);
		Assert.InRange(MathF.Abs(geometry.ComputePathLength() - length), 0f, 0.1f);
		var bounds = geometry.ComputeBounds();
		Assert.InRange(Math.Abs(bounds.X - x), 0d, 0.01d);
		Assert.InRange(Math.Abs(bounds.Y - y), 0d, 0.01d);
		Assert.InRange(Math.Abs(bounds.Width - width), 0d, 0.01d);
		Assert.InRange(Math.Abs(bounds.Height - height), 0d, 0.01d);
	}
}
#endif
