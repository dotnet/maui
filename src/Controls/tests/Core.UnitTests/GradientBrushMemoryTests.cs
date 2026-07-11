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
			var sharedStops = new GradientStopCollection();

			WeakReference weakBrush;
			{
				var brush = new LinearGradientBrush();
				brush.GradientStops = sharedStops;     // installs the non-weak CollectionChanged subscription
				weakBrush = new WeakReference(brush);
				// drop the only strong ref to `brush`; `sharedStops` stays alive.
			}

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			GC.KeepAlive(sharedStops);
		}
	}
}
