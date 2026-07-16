using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PathFigureMemoryLeakTests : BaseTestFixture
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreatePathFigureReference(PathSegmentCollection sharedSegments)
		{
			var figure = new PathFigure
			{
				BindingContext = new byte[1024],
				Segments = sharedSegments
			};

			return new WeakReference(figure);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference AddAndClearSegment(PathFigure figure)
		{
			var segment = new LineSegment(new Point(1, 1));
			figure.Segments.Add(segment);
			figure.Segments.Clear();
			return new WeakReference(segment);
		}

		/// <summary>
		/// Verifies that assigning a shared/long-lived <see cref="PathSegmentCollection"/> to a
		/// transient <see cref="PathFigure"/> does not root the figure. Reproduces issue #36377:
		/// the collection's CollectionChanged (and each segment's PropertyChanged) subscription
		/// used a strong delegate whose target is the PathFigure, so a shared collection kept every
		/// figure ever assigned to it alive.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task PathFigureDoesNotLeakWhenAssignedSharedSegmentCollection()
		{
			// A shared collection (with a segment) that outlives the figure — simulates a
			// static/resource/reused PathSegmentCollection.
			var sharedSegments = new PathSegmentCollection { new LineSegment(new Point(1, 1)) };
			var reference = CreatePathFigureReference(sharedSegments);

			Assert.False(
				await reference.WaitForCollect(),
				"PathFigure should be collected after being dropped, but it was retained by the shared PathSegmentCollection's CollectionChanged/PropertyChanged subscriptions.");

			GC.KeepAlive(sharedSegments);
		}

		[Fact]
		public void DuplicateSegmentsPreserveOccurrenceSubscriptions()
		{
			var segment = new LineSegment(new Point(1, 1));
			var segments = new PathSegmentCollection { segment, segment };
			var figure = new PathFigure { Segments = segments };
			int invalidationCount = 0;
			figure.InvalidatePathSegmentRequested += (_, __) => invalidationCount++;

			segment.Point = new Point(2, 2);
			Assert.Equal(2, invalidationCount);

			segments.Remove(segment);
			invalidationCount = 0;

			segment.Point = new Point(3, 3);
			Assert.Equal(1, invalidationCount);

			segments.Remove(segment);
			invalidationCount = 0;

			segment.Point = new Point(4, 4);
			Assert.Equal(0, invalidationCount);
		}

		[Fact]
		public void ClearingSegmentsUnsubscribesRemovedSegments()
		{
			var segment = new LineSegment(new Point(1, 1));
			var figure = new PathFigure
			{
				Segments = new PathSegmentCollection { segment }
			};
			int invalidationCount = 0;
			figure.InvalidatePathSegmentRequested += (_, __) => invalidationCount++;

			figure.Segments.Clear();
			invalidationCount = 0;

			segment.Point = new Point(2, 2);

			Assert.Equal(0, invalidationCount);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task ClearingSegmentsReleasesRemovedSegments()
		{
			var figure = new PathFigure();
			var reference = AddAndClearSegment(figure);

			Assert.False(
				await reference.WaitForCollect(),
				"PathSegment should be collected after it is removed by Clear.");

			GC.KeepAlive(figure);
		}
	}
}
