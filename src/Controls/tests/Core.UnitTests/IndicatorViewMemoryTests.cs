using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class IndicatorViewMemoryTests : BaseTestFixture
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateIndicatorView(ObservableCollection<string> sharedSource)
		{
			var indicatorView = new IndicatorView { ItemsSource = sharedSource };
			return new WeakReference(indicatorView);
		}

		[Fact]
		public async Task IndicatorViewItemsSourceDoesNotLeak()
		{
			// A long-lived/shared collection, exactly as the issue describes (typically bound to the
			// same collection as a CarouselView). Assigning it to ItemsSource installs a
			// CollectionChanged subscription; without a weak proxy the collection roots the control.
			var sharedSource = new ObservableCollection<string> { "a", "b", "c" };

			var weakIndicatorView = CreateIndicatorView(sharedSource);

			Assert.False(await weakIndicatorView.WaitForCollect(), "IndicatorView should not be alive!");
			GC.KeepAlive(sharedSource);
		}

		[Fact]
		public void ReassigningAndClearingItemsSourceMovesCollectionSubscription()
		{
			var firstSource = new ObservableCollection<string> { "a" };
			var secondSource = new ObservableCollection<string> { "a", "b", "c" };
			var indicatorView = new IndicatorView { ItemsSource = firstSource };

			Assert.Equal(1, indicatorView.Count);

			indicatorView.ItemsSource = secondSource;
			Assert.Equal(3, indicatorView.Count);

			firstSource.Add("stale");
			Assert.Equal(3, indicatorView.Count);

			secondSource.Add("current");
			Assert.Equal(4, indicatorView.Count);

			indicatorView.ItemsSource = null;
			Assert.Equal(0, indicatorView.Count);

			secondSource.Add("detached");
			Assert.Equal(0, indicatorView.Count);
		}
	}
}
