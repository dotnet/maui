using System.Linq;
using Microsoft.Maui.LifecycleEvents;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Lifecycle)]
	public class LifecycleEventOrderTests
	{
		[Fact(DisplayName = "Android lifecycle events fire during startup")]
		public void AndroidLifecycleEventsFireDuringStartup()
		{
			var log = MauiProgram.LifecycleEventLog;

			Assert.Contains(nameof(AndroidLifecycle.OnCreate), log);
			Assert.Contains(nameof(AndroidLifecycle.OnStart), log);
			Assert.Contains(nameof(AndroidLifecycle.OnResume), log);
		}

		[Fact(DisplayName = "Android lifecycle events fire in correct order")]
		public void AndroidLifecycleEventsFireInCorrectOrder()
		{
			var log = MauiProgram.LifecycleEventLog;

			var createIndex = log.IndexOf(nameof(AndroidLifecycle.OnCreate));
			var startIndex = log.IndexOf(nameof(AndroidLifecycle.OnStart));
			var resumeIndex = log.IndexOf(nameof(AndroidLifecycle.OnResume));

			// Expected startup order: OnCreate → OnStart → OnResume
			Assert.True(createIndex < startIndex,
				$"Expected OnCreate before OnStart. Log: [{string.Join(", ", log)}]");
			Assert.True(startIndex < resumeIndex,
				$"Expected OnStart before OnResume. Log: [{string.Join(", ", log)}]");
		}

		[Fact(DisplayName = "OnCreate fires exactly once during startup")]
		public void OnCreateFiresExactlyOnce()
		{
			var count = MauiProgram.LifecycleEventLog.Count(e => e == nameof(AndroidLifecycle.OnCreate));
			Assert.Equal(1, count);
		}
	}
}
