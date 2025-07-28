using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28091SubscribeToUpdates : _IssuesUITest
{
	public override string Issue => "Add Layout Performance Profiler (SubscribeToUpdates)";

	public Issue28091SubscribeToUpdates(TestDevice device) : base(device)
	{
	}

	/// <summary>
	/// This test verifies that layout performance updates are reflected in the HistoryLabel.
	/// It performs resizing actions and asserts that the label text is updated to include the profiling information.
	/// </summary>
	[Test]
	[Category(UITestCategories.Performance)]
	public void HistoryLabel_ShouldUpdate_AfterInteractions()
	{
		App.WaitForElement("WaitForStubControl");
		var initialText = App.FindElement("HistoryLabel")?.GetText();

		// Simulate layout changes
		App.Tap("IncreaseWidthButton");
		App.Tap("IncreaseHeightButton");

		// Wait for history label to update with profiler data
		WaitForPerformanceData();

		// Capture updated history
		var updatedText = App.FindElement("HistoryLabel")?.GetText();

		// Validate history contains expected profiler data
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