using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PathGeometryMemoryTests : BaseTestFixture
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference AssignSharedFiguresAndDrop(PathFigureCollection sharedFigures)
		{
			var geometry = new PathGeometry { Figures = sharedFigures };
			return new WeakReference(geometry);
		}

		static IList GetFigureSubscriptions(PathGeometry geometry)
		{
			const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var subscriptions = typeof(PathGeometry).GetField("_figuresSubscription", flags)?.GetValue(geometry);
			Assert.NotNull(subscriptions);

			return Assert.IsAssignableFrom<IList>(subscriptions.GetType().GetField("_figureSubscriptions", flags)?.GetValue(subscriptions));
		}

		[Fact]
		public async Task PathGeometryDoesNotLeakWhenSharingFigures()
		{
			// A long-lived/shared PathFigureCollection, exactly as the issue describes.
			var sharedFigures = new PathFigureCollection
			{
				new PathFigure()
			};
			var weakGeometry = AssignSharedFiguresAndDrop(sharedFigures);

			Assert.False(await weakGeometry.WaitForCollect(), "PathGeometry should not be alive!");
			GC.KeepAlive(sharedFigures);
		}

		[Fact]
		public void NullFiguresReleasesSubscriptionHelper()
		{
			const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var subscriptionField = typeof(PathGeometry).GetField("_figuresSubscription", flags);
			Assert.NotNull(subscriptionField);
			var geometry = new PathGeometry
			{
				Figures = new PathFigureCollection { new PathFigure() }
			};

			geometry.Figures = null;

			Assert.Null(subscriptionField.GetValue(geometry));
		}

		[Fact]
		public async Task SharedFiguresStillInvalidateAfterGc()
		{
			var existingFigure = new PathFigure();
			var sharedFigures = new PathFigureCollection { existingFigure };
			var geometry = new PathGeometry { Figures = sharedFigures };

			int invalidationCount = 0;
			geometry.InvalidatePathGeometryRequested += (_, __) => invalidationCount++;

			await TestHelpers.Collect();

			existingFigure.IsClosed = true;
			existingFigure.Segments.Add(new LineSegment());

			var addedFigure = new PathFigure();
			sharedFigures.Add(addedFigure);
			addedFigure.IsFilled = false;
			addedFigure.Segments.Add(new LineSegment());

			Assert.Equal(5, invalidationCount);
			GC.KeepAlive(geometry);
		}

		[Fact]
		public void ReplacingFiguresMovesEventSubscriptions()
		{
			var oldFigure = new PathFigure();
			var newFigure = new PathFigure();
			var geometry = new PathGeometry
			{
				Figures = new PathFigureCollection { oldFigure }
			};
			int invalidationCount = 0;
			geometry.InvalidatePathGeometryRequested += (_, __) => invalidationCount++;

			geometry.Figures = new PathFigureCollection { newFigure };

			oldFigure.IsClosed = true;
			oldFigure.Segments.Add(new LineSegment());

			Assert.Equal(0, invalidationCount);

			newFigure.IsClosed = true;
			newFigure.Segments.Add(new LineSegment());

			Assert.Equal(2, invalidationCount);
		}

		[Fact]
		public void ReplacingFigureMovesEventSubscriptions()
		{
			var oldFigure = new PathFigure();
			var newFigure = new PathFigure();
			var figures = new PathFigureCollection { oldFigure };
			var geometry = new PathGeometry { Figures = figures };
			int invalidationCount = 0;
			geometry.InvalidatePathGeometryRequested += (_, __) => invalidationCount++;

			figures[0] = newFigure;
			invalidationCount = 0;

			oldFigure.IsClosed = true;
			oldFigure.Segments.Add(new LineSegment());

			Assert.Equal(0, invalidationCount);

			newFigure.IsClosed = true;
			newFigure.Segments.Add(new LineSegment());

			Assert.Equal(2, invalidationCount);
		}

		[Fact]
		public async Task MovingFiguresReusesSubscriptions()
		{
			var first = new PathFigure();
			var second = new PathFigure();
			var figures = new PathFigureCollection { first, second };
			var geometry = new PathGeometry { Figures = figures };
			var subscriptions = GetFigureSubscriptions(geometry);
			var firstSubscription = subscriptions[0];
			var secondSubscription = subscriptions[1];
			int invalidationCount = 0;
			geometry.InvalidatePathGeometryRequested += (_, __) => invalidationCount++;

			figures.Move(0, 1);

			var movedSubscriptions = GetFigureSubscriptions(geometry);
			Assert.Equal(1, invalidationCount);
			Assert.True(ReferenceEquals(firstSubscription, movedSubscriptions[0]) || ReferenceEquals(firstSubscription, movedSubscriptions[1]));
			Assert.True(ReferenceEquals(secondSubscription, movedSubscriptions[0]) || ReferenceEquals(secondSubscription, movedSubscriptions[1]));

			await TestHelpers.Collect();
			invalidationCount = 0;

			first.IsClosed = true;
			second.Segments.Add(new LineSegment());

			Assert.Equal(2, invalidationCount);
			GC.KeepAlive(geometry);
		}

		[Fact]
		public void ClearingAndReusingFiguresMovesEventSubscriptions()
		{
			var figure = new PathFigure();
			var figures = new PathFigureCollection { figure };
			var geometry = new PathGeometry { Figures = figures };
			int invalidationCount = 0;
			geometry.InvalidatePathGeometryRequested += (_, __) => invalidationCount++;

			figures.Clear();
			invalidationCount = 0;

			figure.IsClosed = true;
			figure.Segments.Add(new LineSegment());

			Assert.Equal(0, invalidationCount);

			figures.Add(figure);
			invalidationCount = 0;

			figure.IsFilled = false;
			figure.Segments.Add(new LineSegment());

			Assert.Equal(2, invalidationCount);
		}
	}
}
