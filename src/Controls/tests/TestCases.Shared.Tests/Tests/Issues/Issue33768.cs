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
	public void NegativeMarginCollectionViewShouldNotCauseExcessiveGC()
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
		// If bug is present, the negative margin causes viewExtendsBeyondScreen to be true,
		// which triggers continuous RequestApplyInsets calls, causing lambda allocations and GC
		Task.Delay(10000).Wait();

		// Get the count after waiting
		var afterWaitStatus = App.FindElement("GCCountLabel").GetText() ?? "";
		int afterWaitCount = GetCountFromStatus(afterWaitStatus);

		System.Diagnostics.Debug.WriteLine($"After 10s GCCount: {afterWaitCount}");

		// Calculate how many GC events happened during idle time
		int gcsDuringIdle = afterWaitCount - initialCount;

		// This test uses a CollectionView with Margin=-50 (negative margin).
		// The negative margin causes native view bounds to extend beyond screen edges.
		// 
		// WITHOUT FIX: viewExtendsBeyondScreen check in SafeAreaExtensions.cs would
		// trigger infinite RequestApplyInsets loop, causing ~60 allocations/sec
		// and 5+ GCs in 30 seconds.
		//
		// WITH FIX: The IRequestInsetsOnTransition guard ensures only Shell fragments
		// during transitions get the re-apply behavior, so no infinite loop occurs.
		// 
		// Threshold of 2 allows for minimal GC from normal app activity
		Assert.That(gcsDuringIdle, Is.LessThan(2),
			$"CollectionView with negative margin should not cause excessive GC. " +
			$"Initial: {initialCount}, After 10s: {afterWaitCount}, GCs during idle: {gcsDuringIdle}. " +
			$"Excessive GC indicates regression in RequestApplyInsets handling (Issue #33768).");
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
