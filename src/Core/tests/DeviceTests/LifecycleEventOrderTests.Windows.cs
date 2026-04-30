using System.Linq;
using Microsoft.Maui.LifecycleEvents;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Lifecycle)]
	public class LifecycleEventOrderTests
	{
		[Fact(DisplayName = "Windows lifecycle events fire during startup")]
		public void WindowsLifecycleEventsFireDuringStartup()
		{
			var log = MauiProgram.LifecycleEventLog;

			Assert.Contains(nameof(WindowsLifecycle.OnAppInstanceActivated), log);
			Assert.Contains(nameof(WindowsLifecycle.OnLaunching), log);
			Assert.Contains(nameof(WindowsLifecycle.OnLaunched), log);
			Assert.Contains(nameof(WindowsLifecycle.OnWindowCreated), log);
		}

		[Fact(DisplayName = "Windows lifecycle events fire in correct order")]
		public void WindowsLifecycleEventsFireInCorrectOrder()
		{
			var log = MauiProgram.LifecycleEventLog;

			var activatedIndex = log.IndexOf(nameof(WindowsLifecycle.OnAppInstanceActivated));
			var launchingIndex = log.IndexOf(nameof(WindowsLifecycle.OnLaunching));
			var windowCreatedIndex = log.IndexOf(nameof(WindowsLifecycle.OnWindowCreated));
			var launchedIndex = log.IndexOf(nameof(WindowsLifecycle.OnLaunched));

			// Expected startup order: OnAppInstanceActivated → OnLaunching → OnWindowCreated → OnLaunched
			Assert.True(activatedIndex < launchingIndex,
				$"Expected OnAppInstanceActivated before OnLaunching. Log: [{string.Join(", ", log)}]");
			Assert.True(launchingIndex < windowCreatedIndex,
				$"Expected OnLaunching before OnWindowCreated. Log: [{string.Join(", ", log)}]");
			Assert.True(windowCreatedIndex < launchedIndex,
				$"Expected OnWindowCreated before OnLaunched. Log: [{string.Join(", ", log)}]");
		}

		[Fact(DisplayName = "OnAppInstanceActivated fires exactly once during startup")]
		public void OnAppInstanceActivatedFiresExactlyOnce()
		{
			var count = MauiProgram.LifecycleEventLog.Count(e => e == nameof(WindowsLifecycle.OnAppInstanceActivated));
			Assert.Equal(1, count);
		}
	}
}
