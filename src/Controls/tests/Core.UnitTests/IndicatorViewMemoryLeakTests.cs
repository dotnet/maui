using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class IndicatorViewMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that an IndicatorView bound to a long-lived ObservableCollection via ItemsSource
		/// does not leak after the view is discarded without resetting ItemsSource. Reproduces issue #36724.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task IndicatorViewDoesNotLeakWhenItemsSourceBoundToSharedCollection()
		{
			// A long-lived / shared collection that outlives the IndicatorView.
			var sharedSource = new ObservableCollection<string> { "a", "b", "c" };

			WeakReference weakIndicatorView;
			{
				var indicatorView = new IndicatorView { ItemsSource = sharedSource };
				weakIndicatorView = new WeakReference(indicatorView);
				// Drop the only strong reference to the IndicatorView; sharedSource stays alive.
			}

			Assert.False(await weakIndicatorView.WaitForCollect(), "IndicatorView should not be alive!");
			GC.KeepAlive(sharedSource);
		}
	}
}
