using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GradientBrushMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that a transient <see cref="GradientBrush"/> is not kept alive by a shared,
		/// long-lived <see cref="GradientStopCollection"/> assigned to its <see cref="GradientBrush.GradientStops"/>.
		/// Reproduces issue #36363: the brush subscribed to the collection's CollectionChanged with a
		/// strong delegate and only detached on property reassignment, so a shared collection rooted the brush.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task GradientBrushDoesNotLeakWhenGradientStopsShared()
		{
			// A long-lived collection that outlives the brush (e.g. a shared/static GradientStopCollection).
			var sharedStops = new GradientStopCollection
			{
				new GradientStop(Colors.Red, 0),
				new GradientStop(Colors.Blue, 1),
			};

			WeakReference CreateBrushReference()
			{
				var brush = new LinearGradientBrush
				{
					GradientStops = sharedStops
				};

				return new WeakReference(brush);
			}

			var reference = CreateBrushReference();

			Assert.False(await reference.WaitForCollect(), "GradientBrush should not be alive!");

			GC.KeepAlive(sharedStops);
		}
	}
}
