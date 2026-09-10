using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// Each of these types subscribes to a child collection and to the children inside it. When that
	// collection is long-lived -- shared between owners, or declared in a ResourceDictionary -- a
	// non-weak subscription roots every owner ever assigned it. None of these types set Parent on
	// their children, so the event subscriptions are the only retention path under test.
	public class ShapesWeakEventTests : BaseTestFixture
	{
		// https://github.com/dotnet/maui/issues/37583
		[Fact]
		public async Task SharedCollectionDoesNotRootTransformGroup()
		{
			var shared = new TransformCollection { new RotateTransform { Angle = 45 } };

			var reference = CreateTransformGroup(shared);

			Assert.False(await reference.WaitForCollect(), "TransformGroup should not be alive!");
			GC.KeepAlive(shared);
		}

		// https://github.com/dotnet/maui/issues/37582
		[Fact]
		public async Task SharedCollectionDoesNotRootGeometryGroup()
		{
			var shared = new GeometryCollection { new EllipseGeometry { RadiusX = 1, RadiusY = 2 } };

			var reference = CreateGeometryGroup(shared);

			Assert.False(await reference.WaitForCollect(), "GeometryGroup should not be alive!");
			GC.KeepAlive(shared);
		}

		// https://github.com/dotnet/maui/issues/37584
		[Fact]
		public async Task SharedCollectionDoesNotRootPathFigure()
		{
			var shared = new PathSegmentCollection { new LineSegment { Point = new Point(1, 1) } };

			var reference = CreatePathFigure(shared);

			Assert.False(await reference.WaitForCollect(), "PathFigure should not be alive!");
			GC.KeepAlive(shared);
		}

		// https://github.com/dotnet/maui/issues/37585
		[Fact]
		public async Task SharedCollectionDoesNotRootPathGeometry()
		{
			var shared = new PathFigureCollection { new PathFigure { StartPoint = new Point(1, 1) } };

			var reference = CreatePathGeometry(shared);

			Assert.False(await reference.WaitForCollect(), "PathGeometry should not be alive!");
			GC.KeepAlive(shared);
		}

		// The guards below prove the weak subscriptions still deliver events; without them the leak
		// tests above would also pass if the subscriptions had simply been dropped.
		[Fact]
		public void TransformGroupStillRecomputesWhenChildChanges()
		{
			var rotate = new RotateTransform { Angle = 0 };
			var group = new TransformGroup { Children = new TransformCollection { rotate } };

			var before = group.Value;
			rotate.Angle = 90;

			Assert.NotEqual(before, group.Value);
			GC.KeepAlive(group);
		}

		[Fact]
		public void GeometryGroupStillInvalidatesWhenChildChanges()
		{
			var ellipse = new EllipseGeometry { RadiusX = 1, RadiusY = 1 };
			var group = new GeometryGroup { Children = new GeometryCollection { ellipse } };

			bool invalidated = false;
			group.InvalidateGeometryRequested += (s, e) => invalidated = true;

			ellipse.RadiusX = 5;

			Assert.True(invalidated);
			GC.KeepAlive(group);
		}

		[Fact]
		public void GeometryGroupStillInvalidatesWhenCollectionChanges()
		{
			var group = new GeometryGroup { Children = new GeometryCollection() };

			bool invalidated = false;
			group.InvalidateGeometryRequested += (s, e) => invalidated = true;

			group.Children.Add(new EllipseGeometry { RadiusX = 1, RadiusY = 1 });

			Assert.True(invalidated);
			GC.KeepAlive(group);
		}

		[Fact]
		public void PathFigureStillInvalidatesWhenSegmentChanges()
		{
			var segment = new LineSegment { Point = new Point(0, 0) };
			var figure = new PathFigure { Segments = new PathSegmentCollection { segment } };

			bool invalidated = false;
			figure.InvalidatePathSegmentRequested += (s, e) => invalidated = true;

			segment.Point = new Point(5, 5);

			Assert.True(invalidated);
			GC.KeepAlive(figure);
		}

		// PathGeometry subscribes to each figure twice: PropertyChanged and the internal
		// InvalidatePathSegmentRequested. Both are now weak, so both are covered here.
		[Fact]
		public void PathGeometryStillInvalidatesWhenFigureChanges()
		{
			var figure = new PathFigure { StartPoint = new Point(0, 0) };
			var geometry = new PathGeometry { Figures = new PathFigureCollection { figure } };

			bool invalidated = false;
			geometry.InvalidatePathGeometryRequested += (s, e) => invalidated = true;

			figure.StartPoint = new Point(5, 5);

			Assert.True(invalidated);
			GC.KeepAlive(geometry);
		}

		[Fact]
		public void PathGeometryStillInvalidatesWhenSegmentInsideFigureChanges()
		{
			var segment = new LineSegment { Point = new Point(0, 0) };
			var figure = new PathFigure { Segments = new PathSegmentCollection { segment } };
			var geometry = new PathGeometry { Figures = new PathFigureCollection { figure } };

			bool invalidated = false;
			geometry.InvalidatePathGeometryRequested += (s, e) => invalidated = true;

			segment.Point = new Point(5, 5);

			Assert.True(invalidated);
			GC.KeepAlive(geometry);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateTransformGroup(TransformCollection children) =>
			new WeakReference(new TransformGroup { Children = children });

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateGeometryGroup(GeometryCollection children) =>
			new WeakReference(new GeometryGroup { Children = children });

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreatePathFigure(PathSegmentCollection segments) =>
			new WeakReference(new PathFigure { Segments = segments });

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreatePathGeometry(PathFigureCollection figures) =>
			new WeakReference(new PathGeometry { Figures = figures });
	}
}
