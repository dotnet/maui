using System.Linq;
using Microsoft.Maui.LifecycleEvents;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Lifecycle)]
	public class LifecycleEventOrderTests
	{
		[Fact(DisplayName = "iOS lifecycle events fire during startup")]
		public void iOSLifecycleEventsFireDuringStartup()
		{
			var log = MauiProgram.LifecycleEventLog;

			Assert.Contains(nameof(iOSLifecycle.FinishedLaunching), log);
		}

		[Fact(DisplayName = "FinishedLaunching fires before OnActivated")]
		public void FinishedLaunchingFiresBeforeOnActivated()
		{
			var log = MauiProgram.LifecycleEventLog;

			var launchIndex = log.IndexOf(nameof(iOSLifecycle.FinishedLaunching));
			Assert.True(launchIndex >= 0, "FinishedLaunching should have fired");

			var activatedIndex = log.IndexOf(nameof(iOSLifecycle.OnActivated));
			if (activatedIndex >= 0)
			{
				Assert.True(launchIndex < activatedIndex,
					$"Expected FinishedLaunching before OnActivated. Log: [{string.Join(", ", log)}]");
			}
		}

		[Fact(DisplayName = "FinishedLaunching fires exactly once during startup")]
		public void FinishedLaunchingFiresExactlyOnce()
		{
			var count = MauiProgram.LifecycleEventLog.Count(e => e == nameof(iOSLifecycle.FinishedLaunching));
			Assert.Equal(1, count);
		}
	}
}
