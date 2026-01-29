using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33768 : _IssuesUITest
{
	public override string Issue => "Performance degradation on Android caused by Infinite Layout Loop (RequestApplyInsets)";

	public Issue33768(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Layout)]
	public void ScrollableContentShouldNotCauseExcessiveGC()
	{
		// Wait for the page to load
		App.WaitForElement("GCCountLabel");
		App.WaitForElement("TitleLabel");

		// Wait a moment for initial layout and GC baseline to stabilize
		Task.Delay(1000).Wait();

		// Get initial GC count
		var initialStatus = App.FindElement("GCCountLabel").GetText() ?? "";
		int initialCount = GetCountFromStatus(initialStatus);

		System.Diagnostics.Debug.WriteLine($"Initial GCCount: {initialCount}");

		// Wait 10 seconds while idle
		// If a regression occurs, views positioned beyond screen bounds would trigger
		// continuous RequestApplyInsets calls, causing lambda allocations and GC
		Task.Delay(10000).Wait();

		// Get the count after waiting
		var afterWaitStatus = App.FindElement("GCCountLabel").GetText() ?? "";
		int afterWaitCount = GetCountFromStatus(afterWaitStatus);

		System.Diagnostics.Debug.WriteLine($"After 10s GCCount: {afterWaitCount}");

		// Calculate how many GC events happened during idle time
		int gcsDuringIdle = afterWaitCount - initialCount;

		// This test verifies that scrollable content doesn't trigger excessive GC.
		// Issue #33768 was closed as duplicate of #33731, both caused by PR #33285's
		// viewExtendsBeyondScreen check. The fix ensures that:
		// 1. Views outside Shell don't get transition inset re-application
		// 2. Views completely off-screen are skipped entirely
		// 
		// We use a threshold of 2 to detect regressions while allowing for minimal GC
		// from normal app activity
		Assert.That(gcsDuringIdle, Is.LessThan(2),
			$"Scrollable content should not cause excessive GC. " +
			$"Initial: {initialCount}, After 10s: {afterWaitCount}, GCs during idle: {gcsDuringIdle}. " +
			$"Excessive GC may indicate a regression in RequestApplyInsets handling.");
	}
	}

	private static int GetCountFromStatus(string status)
	{
		// Format: "GCCount: X"
		var parts = status.Split(':');
		if (parts.Length >= 2 && int.TryParse(parts[1].Trim(), out int count))
		{
			return count;
		}
		return 0;
	}
}
