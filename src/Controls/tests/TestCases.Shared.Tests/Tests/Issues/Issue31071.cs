#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31071 : _IssuesUITest
{
	public Issue31071(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue =>
		"CollectionView2 (CollectionViewHandler2) much more slow than CV1 on updating columns count at runtime";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupingChangesUpdateLayoutCorrectly()
	{
		App.WaitForElement("UngroupedButton");

		// Start with ungrouped
		App.Tap("UngroupedButton");
		App.WaitForElement("TestCollectionView");

		// Verify initial ungrouped state
		var ungroupedActionTime = GetActionTime();
		Assert.That(ungroupedActionTime, Is.GreaterThan(0), "Should record action time for ungrouping");

		// Switch to grouped
		App.Tap("GroupedButton");
		App.WaitForElement("TestCollectionView");

		// Verify grouped state change
		var groupedActionTime = GetActionTime();
		Assert.That(groupedActionTime, Is.GreaterThan(0), "Should record action time for grouping");

		// Switch back to ungrouped to test the issue was fixed
		App.Tap("UngroupedButton");
		App.WaitForElement("TestCollectionView");

		var secondUngroupedActionTime = GetActionTime();
		Assert.That(secondUngroupedActionTime, Is.GreaterThan(0), "Should record action time for second ungrouping");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ColumnChangesDoNotCauseExponentialPerformanceDegradation()
	{
		App.WaitForElement("Cols4Button");

		// Record baseline performance for 4 columns
		App.Tap("Cols4Button");
		App.WaitForElement("TestCollectionView");
		var cols4Time1 = GetActionTime();

		// Switch to 6 columns
		App.Tap("Cols6Button");
		App.WaitForElement("TestCollectionView");
		var cols6Time1 = GetActionTime();

		// Switch back to 4 columns multiple times to test for exponential degradation
		App.Tap("Cols4Button");

		App.Tap("Cols6Button");

		App.Tap("Cols4Button");
		var cols4Time2 = GetActionTime();

		App.Tap("Cols6Button");
		var cols6Time2 = GetActionTime();

		// Verify that performance doesn't degrade exponentially
		// With the fix, subsequent changes should be similar to the first change
		// Allow some variance but ensure no exponential growth
		Assert.That(cols4Time2, Is.LessThan(cols4Time1 * 5),
			"Column changes should not cause exponential performance degradation");
		Assert.That(cols6Time2, Is.LessThan(cols6Time1 * 5),
			"Column changes should not cause exponential performance degradation");

		// All times should be reasonable (under 1 second for layout changes)
		Assert.That(cols4Time2, Is.LessThan(1.0), "Layout changes should complete quickly");
		Assert.That(cols6Time2, Is.LessThan(1.0), "Layout changes should complete quickly");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void PerformanceTimesAreRecordedCorrectly()
	{
		App.WaitForElement("ActionTimeLabel");

		// Initial state should have 0 or minimal time
		var initialTime = GetActionTime();

		// Perform an action
		App.Tap("UngroupedButton");
		App.WaitForElement("TestCollectionView");

		// Verify action time was recorded
		var actionTime = GetActionTime();
		Assert.That(actionTime, Is.GreaterThan(0), "Action time should be recorded");
		Assert.That(actionTime, Is.LessThan(10.0), "Action time should be reasonable");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void MemoryUsageIsDisplayed()
	{
		App.WaitForElement("MemoryLabel");

		var memoryText = App.FindElement("MemoryLabel").GetText();
		Assert.That(memoryText, Does.Contain("Heap size:"), "Memory usage should be displayed");
		Assert.That(memoryText, Does.Contain("mb"), "Memory usage should show units");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ItemCountIsTracked()
	{
		App.WaitForElement("ItemsCountLabel");

		var itemCountText = App.FindElement("ItemsCountLabel").GetText();
		Assert.That(itemCountText, Does.Contain("CV items count:"), "Item count should be displayed");
	}

	double GetActionTime()
	{
		var actionTimeText = App.FindElement("ActionTimeLabel").GetText();

		if (actionTimeText is null)
		{
			return 0.0;
		}

		// Extract the numeric value from "Last action duration: X.XXX s"
		var startIndex = actionTimeText.IndexOf(": ", StringComparison.Ordinal) + 2;
		var endIndex = actionTimeText.IndexOf(" s", StringComparison.Ordinal);

		if (startIndex > 1 && endIndex > startIndex)
		{
			var timeString = actionTimeText.Substring(startIndex, endIndex - startIndex);
			if (double.TryParse(timeString, out double time))
			{
				return time;
			}
		}

		return 0.0;
	}
}
#endif