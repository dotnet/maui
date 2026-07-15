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
	}
}
