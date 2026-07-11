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
	}
}
