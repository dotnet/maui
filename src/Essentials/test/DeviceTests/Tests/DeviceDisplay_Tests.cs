using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("DeviceDisplay")]
	public class DeviceDisplay_Tests
	{
		[Fact]
		public Task Screen_Metrics_Are_Not_Null()
		{
			return Utils.OnMainThread(() =>
			{
				var metrics = DeviceDisplay.MainDisplayInfo;

				Assert.True(metrics.Width > 0);
				Assert.True(metrics.Height > 0);
				Assert.True(metrics.Density > 0);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Locks()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Unlocks_Without_Locking()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Locks_Only_Once()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);
				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}
	}
}
