using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GradientBrushTests : BaseTestFixture
	{
		// A GradientStopCollection that outlives the brush must not root it through CollectionChanged.
		// The collection is left empty so that GradientStop.Parent (a strong reference by design) cannot
		// mask the subscription being tested.
		// https://github.com/dotnet/maui/issues/37581
		[Fact]
		public async Task SharedEmptyCollectionDoesNotRootBrush()
		{
			var shared = new GradientStopCollection();

			var reference = CreateBrush(shared);

			Assert.False(await reference.WaitForCollect(), "GradientBrush should not be alive!");
			GC.KeepAlive(shared);
		}

		// When several brushes share one collection, every brush but the last hands GradientStop.Parent
		// over to its successor, so nothing but the stop's PropertyChanged subscription can retain it.
		// https://github.com/dotnet/maui/issues/37581
		[Fact]
		public async Task SharedPopulatedCollectionDoesNotRootDiscardedBrush()
		{
			var shared = new GradientStopCollection
			{
				new GradientStop { Color = Colors.Red, Offset = 0.0f },
				new GradientStop { Color = Colors.Blue, Offset = 1.0f }
			};

			var reference = CreateBrush(shared);

			// A second brush takes ownership of the stops (Parent), leaving the first retained only by
			// the events it subscribed to.
			var successor = new LinearGradientBrush { GradientStops = shared };

			Assert.False(await reference.WaitForCollect(), "Discarded GradientBrush should not be alive!");
			GC.KeepAlive(shared);
			GC.KeepAlive(successor);
		}

		// Clear() raises a Reset with no OldItems; the stops it removed must still be detached.
		// https://github.com/dotnet/maui/issues/38179
		[Fact]
		public async Task RetainedStopDoesNotRootBrushAfterClear()
		{
			var retainedStop = new GradientStop { Color = Colors.Red, Offset = 0.0f };

			var reference = CreateBrushAndClear(retainedStop);

			Assert.False(await reference.WaitForCollect(), "GradientBrush should not be alive after Clear!");
			GC.KeepAlive(retainedStop);
		}

		[Fact]
		public void ClearDetachesStopParent()
		{
			var stop = new GradientStop { Color = Colors.Red, Offset = 0.0f };
			var brush = new LinearGradientBrush();
			brush.GradientStops.Add(stop);

			Assert.Same(brush, stop.Parent);

			brush.GradientStops.Clear();

			Assert.Null(stop.Parent);
		}

		[Fact]
		public void ClearStopsInvalidationFromRemovedStop()
		{
			var stop = new GradientStop { Color = Colors.Red, Offset = 0.0f };
			var brush = new LinearGradientBrush();
			brush.GradientStops.Add(stop);
			brush.GradientStops.Clear();

			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (s, e) => invalidated = true;

			stop.Color = Colors.Blue;

			Assert.False(invalidated);
			GC.KeepAlive(brush);
		}

		// Guards the weak subscriptions actually still deliver events.
		[Fact]
		public void AttachedStopStillRaisesInvalidation()
		{
			var stop = new GradientStop { Color = Colors.Red, Offset = 0.0f };
			var brush = new LinearGradientBrush();
			brush.GradientStops.Add(stop);

			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (s, e) => invalidated = true;

			stop.Color = Colors.Blue;

			Assert.True(invalidated);
			GC.KeepAlive(brush);
		}

		[Fact]
		public void CollectionChangesStillRaiseInvalidation()
		{
			var brush = new LinearGradientBrush();

			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (s, e) => invalidated = true;

			brush.GradientStops.Add(new GradientStop { Color = Colors.Red, Offset = 0.0f });

			Assert.True(invalidated);
			GC.KeepAlive(brush);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateBrush(GradientStopCollection stops) =>
			new WeakReference(new LinearGradientBrush { GradientStops = stops });

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateBrushAndClear(GradientStop stop)
		{
			var brush = new LinearGradientBrush();
			brush.GradientStops.Add(stop);
			brush.GradientStops.Clear();
			return new WeakReference(brush);
		}
	}
}
