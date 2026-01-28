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
	public void TabbedPageShouldNotContinuouslyTriggerLayout()
	{
		// Wait for the TabbedPage to load
		App.WaitForElement("LayoutCountLabel");
		App.WaitForElement("Tab1Label");

		// Wait a moment for initial layout to complete
		Task.Delay(500).Wait();

		// Get initial layout count
		var initialStatus = App.FindElement("LayoutCountLabel").GetText() ?? "";
		int initialCount = GetCountFromStatus(initialStatus);
		
		System.Diagnostics.Debug.WriteLine($"Initial LayoutCount: {initialCount}");

		// Wait 3 seconds while idle
		// If the bug is present, layout/inset requests happen in an infinite loop
		Task.Delay(3000).Wait();

		// Get the count after waiting
		var afterWaitStatus = App.FindElement("LayoutCountLabel").GetText() ?? "";
		int afterWaitCount = GetCountFromStatus(afterWaitStatus);

		System.Diagnostics.Debug.WriteLine($"After 3s LayoutCount: {afterWaitCount}");

		// Calculate how many layout events happened during idle time
		int layoutsDuringIdle = afterWaitCount - initialCount;

		// The bug causes an infinite loop of RequestApplyInsets calls, which triggers
		// continuous GC (every ~5-6 seconds). This test monitors GlobalLayout events
		// as an indirect indicator. Normal TabbedPage layout produces ~50-200 events
		// during idle due to animation-related layouts.
		// 
		// With the bug (infinite loop), we'd see much higher activity AND continuous GC.
		// We use a threshold of 500 to detect extreme layout thrashing while allowing
		// for normal TabbedPage behavior. The real verification is via logcat GC analysis.
		Assert.That(layoutsDuringIdle, Is.LessThan(500),
			$"Layout activity is abnormally high. " +
			$"Initial: {initialCount}, After 3s: {afterWaitCount}, Layouts during idle: {layoutsDuringIdle}. " +
			$"Check logcat for continuous GC events every 5-6 seconds (the actual bug symptom).");
	}

	private static int GetCountFromStatus(string status)
	{
		// Format: "LayoutCount: X"
		var parts = status.Split(':');
		if (parts.Length >= 2 && int.TryParse(parts[1].Trim(), out int count))
		{
			return count;
		}
		return 0;
	}
}
