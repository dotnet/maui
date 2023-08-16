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
		[UIFact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public void Screen_Metrics_Are_Not_Null()
		{
				var metrics = DeviceDisplay.MainDisplayInfo;

				Assert.True(metrics.Width > 0);
				Assert.True(metrics.Height > 0);
				Assert.True(metrics.Density > 0);
		}

		[UIFact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public void ScreenLock_Locks()
		{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
		}

		[UIFact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public void ScreenLock_Unlocks_Without_Locking()
		{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
		}

		[UIFact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public void ScreenLock_Locks_Only_Once()
		{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);
				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
		}
	}
}
