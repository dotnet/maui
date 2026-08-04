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
			// A shared/long-lived GradientStopCollection (e.g. one declared once in a
			// ResourceDictionary and reused by many brushes) must not root the brushes
			// that subscribe to its CollectionChanged event.
			var sharedStops = new GradientStopCollection();

			WeakReference weakBrush;
			{
				var brush = new LinearGradientBrush { GradientStops = sharedStops };
				weakBrush = new WeakReference(brush);
			}

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			GC.KeepAlive(sharedStops);
		}
	}
}
