using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33731 : _IssuesUITest
{
	public override string Issue => "Continuous GC logs on TabbedPage in MAUI 10.0.30";

	public Issue33731(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageShouldNotCauseExcessiveGC()
	{
		// Wait for the TabbedPage to load
		App.WaitForElement("GCCountLabel");
		App.WaitForElement("Tab1Label");

		// Wait a moment for initial layout and GC baseline to stabilize
		Task.Delay(1000).Wait();

		// Get initial GC count
		var initialStatus = App.FindElement("GCCountLabel").GetText() ?? "";
		int initialCount = GetCountFromStatus(initialStatus);

		System.Diagnostics.Debug.WriteLine($"Initial GCCount: {initialCount}");

		// Wait 10 seconds while idle
		// If the bug is present, GC happens every ~5-6 seconds due to lambda allocations
		// from the infinite RequestApplyInsets loop
		Task.Delay(10000).Wait();

		// Get the count after waiting
		var afterWaitStatus = App.FindElement("GCCountLabel").GetText() ?? "";
		int afterWaitCount = GetCountFromStatus(afterWaitStatus);

		System.Diagnostics.Debug.WriteLine($"After 10s GCCount: {afterWaitCount}");

		// Calculate how many GC events happened during idle time
		int gcsDuringIdle = afterWaitCount - initialCount;

		// If the bug is present:
		// - Lambda allocations from view.Post(() => RequestApplyInsets) ~60 times/sec
		// - This causes GC every ~5-6 seconds
		// - In 10 seconds, we'd see 1-2 GC events
		// 
		// If fixed:
		// - No lambda allocations in the loop
		// - 0 GC events during idle
		// 
		// We use a threshold of 2 to detect the bug while allowing for minimal GC
		// from normal app activity
		Assert.That(gcsDuringIdle, Is.LessThan(2),
			$"TabbedPage should not cause excessive GC. " +
			$"Initial: {initialCount}, After 10s: {afterWaitCount}, GCs during idle: {gcsDuringIdle}. " +
			$"Excessive GC indicates infinite RequestApplyInsets loop from PR #33285.");
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
