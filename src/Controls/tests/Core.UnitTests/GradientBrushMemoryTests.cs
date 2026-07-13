using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GradientBrushMemoryTests : BaseTestFixture
	{
		[Fact]
		public async Task GradientBrushDoesNotLeakWhenSharingGradientStops()
		{
			// A long-lived/shared GradientStopCollection, exactly as the issue describes.
			var sharedStops = new GradientStopCollection
			{
				new GradientStop()
			};

			WeakReference weakBrush;
			{
				var brush = new LinearGradientBrush();
				brush.GradientStops = sharedStops;
				weakBrush = new WeakReference(brush);
			}

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task GradientStopChangesStillInvalidateAfterGc()
		{
			var stop = new GradientStop();
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { stop }
			};
			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidated = true;

			await TestHelpers.Collect();

			stop.Offset = 0.5f;

			Assert.True(invalidated);
			GC.KeepAlive(brush);
		}

		[Fact]
		public async Task SharedGradientStopsInvalidateEachLiveBrushAfterGc()
		{
			var stop = new GradientStop();
			var sharedStops = new GradientStopCollection { stop };
			var firstBrush = new LinearGradientBrush { GradientStops = sharedStops };
			var secondBrush = new LinearGradientBrush { GradientStops = sharedStops };
			int firstInvalidationCount = 0;
			int secondInvalidationCount = 0;
			firstBrush.InvalidateGradientBrushRequested += (_, __) => firstInvalidationCount++;
			secondBrush.InvalidateGradientBrushRequested += (_, __) => secondInvalidationCount++;

			await TestHelpers.Collect();

			stop.Offset = 0.5f;

			Assert.Equal(1, firstInvalidationCount);
			Assert.Equal(1, secondInvalidationCount);
			GC.KeepAlive(firstBrush);
			GC.KeepAlive(secondBrush);
		}

		[Fact]
		public void RemovingAndReplacingGradientStopsMovesSubscriptions()
		{
			var removedStop = new GradientStop { Offset = 0.1f };
			var retainedStop = new GradientStop { Offset = 0.2f };
			var oldStops = new GradientStopCollection { removedStop, retainedStop };
			var brush = new LinearGradientBrush { GradientStops = oldStops };
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			oldStops.Remove(removedStop);
			invalidationCount = 0;

			removedStop.Offset = 0.3f;
			Assert.Equal(0, invalidationCount);

			retainedStop.Offset = 0.4f;
			Assert.Equal(1, invalidationCount);

			var replacementStop = new GradientStop { Offset = 0.1f };
			brush.GradientStops = new GradientStopCollection { replacementStop };
			invalidationCount = 0;

			retainedStop.Offset = 0.5f;
			oldStops.Add(new GradientStop());

			Assert.Equal(0, invalidationCount);

			replacementStop.Offset = 0.6f;

			Assert.Equal(1, invalidationCount);
		}

		[Fact]
		public void ClearingGradientStopsAllowsReentrantReplacement()
		{
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { new GradientStop() }
			};
			bool replaced = false;
			brush.InvalidateGradientBrushRequested += (_, __) =>
			{
				if (!replaced && brush.GradientStops.Count == 0)
				{
					replaced = true;
					brush.GradientStops = new GradientStopCollection { new GradientStop() };
				}
			};

			brush.GradientStops.Clear();

			Assert.True(replaced);
			Assert.Single(brush.GradientStops);
		}

		[Fact]
		public async Task ClearingAndReusingGradientStopsKeepsNewStopsSubscribed()
		{
			var stops = new GradientStopCollection { new GradientStop() };
			var brush = new LinearGradientBrush
			{
				GradientStops = stops
			};
			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidated = true;

			stops.Clear();
			var newStop = new GradientStop();
			stops.Add(newStop);

			await TestHelpers.Collect();
			invalidated = false;

			newStop.Offset = 0.5f;

			Assert.True(invalidated);
			GC.KeepAlive(brush);
		}
	}
}
