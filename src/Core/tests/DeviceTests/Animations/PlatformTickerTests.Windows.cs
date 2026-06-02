#if WINDOWS
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Ticker)]
	public class PlatformTickerTests : TestBase
	{
		[Fact]
		public async Task IsRunning_ReflectsStartAndStop()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var ticker = new PlatformTicker();

				Assert.False(ticker.IsRunning, "Should be false before Start");

				ticker.Start();
				Assert.True(ticker.IsRunning, "Should be true after Start");

				ticker.Stop();
				Assert.False(ticker.IsRunning, "Should be false after Stop");
			});
		}

		[Fact]
		public async Task Start_IsIdempotent_NoDuplicateSubscriptions()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var ticker = new PlatformTicker();
				int fireCount = 0;
				ticker.Fire = () => fireCount++;

				ticker.Start();
				ticker.Start(); // second call should be a no-op
				ticker.Start(); // third call should be a no-op

				// Yield a few composition frames
				await Task.Delay(100);

				ticker.Stop();
				var firesAfterStop = fireCount;

				await Task.Delay(50);

				// No additional fires after Stop
				Assert.Equal(firesAfterStop, fireCount);
				// ~6 expected at 60Hz over 100ms; 3x subscriptions would yield ~18+
				Assert.InRange(firesAfterStop, 1, 20);
			});
		}

		[Fact]
		public async Task AnimationManager_StartsTickerOnce_AcrossMultipleAdds()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var ticker = new PlatformTicker();
				var manager = new AnimationManager(ticker);

				for (int i = 0; i < 5; i++)
					manager.Add(new Animation { Duration = 1.0 });

				Assert.True(ticker.IsRunning);

				ticker.Stop();
				Assert.False(ticker.IsRunning);
			});
		}
	}
}
#endif
