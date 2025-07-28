using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28091GetHistory : _IssuesUITest
{
	public override string Issue => "Add Layout Performance Profiler (GetHistory)";

	public Issue28091GetHistory(TestDevice device) : base(device)
	{
	}

	/// <summary>
	/// This test verifies that layout performance data is correctly recorded by the PerformanceProfiler 
	/// and displayed when the "Show History" button is tapped. It simulates layout changes by resizing 
	/// a Rectangle element, then ensures that the resulting layout update history includes profiling 
	/// data such as element name and duration.
	/// </summary>
	[Test]
	[Category(UITestCategories.Performance)]
	public void ShowHistory_ShouldDisplay_ProfilerDataAfterResizing()
	{
		App.WaitForElement("WaitForStubControl");
		var initialText = App.FindElement("HistoryLabel").GetText();

		// Trigger some layout updates
		App.Tap("IncreaseWidthButton");
		App.Tap("IncreaseHeightButton");
		App.Tap("DecreaseWidthButton");
		App.Tap("DecreaseHeightButton");

		// Show the recorded history
		App.Tap("ShowHistoryButton");

		// Wait for history label to update with profiler data
		WaitForPerformanceData();

		// Capture updated history
		var updatedText = App.FindElement("HistoryLabel")?.GetText();

		// Validate output contains expected profiler terms and differs from initial state
		Assert.That(updatedText, Is.Not.Null
			.And.Contains("Rectangle")
			.And.Contains("Duration")
			.IgnoreCase);
		Assert.That(updatedText, Is.Not.EqualTo(initialText));
	}

	void WaitForPerformanceData()
	{
		const int maxRetries = 5;
		const int delayMs = 500;

		for (int i = 0; i < maxRetries; i++)
		{
			try
			{
				var historyText = App.FindElement("HistoryLabel")?.GetText();
				if (!string.IsNullOrEmpty(historyText) &&
				    (historyText.Contains("Rectangle", StringComparison.OrdinalIgnoreCase) ||
				     historyText.Contains("Duration", StringComparison.OrdinalIgnoreCase)))
				{
					return; // Found
				}
			}
			catch
			{
				// Ignore exceptions during polling
			}

			Thread.Sleep(delayMs);
		}

		// If we reach here, the retry failed, let the main test assertions handle it
	}
}