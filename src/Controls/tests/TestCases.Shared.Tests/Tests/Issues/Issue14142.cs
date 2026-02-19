#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14142 : _IssuesUITest
{
	public Issue14142(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Windows] Window was already activated when minimize and restore from taskbar";

	[Test]
	[Category(UITestCategories.Window)]
	public void WindowLifecycleEventsFireOnMinimizeRestore()
	{
		App.WaitForElement("RunTestButton");
		App.WaitForElement("StatusLabel");

		// Check initial state - should show "Events subscribed" if OnNavigatedTo ran
		var initialResult = App.WaitForElement("ResultLabel").GetText();
		TestContext.WriteLine($"Initial ResultLabel: {initialResult}");

		// Run the minimize/restore test
		App.Tap("RunTestButton");

		// Wait for test to complete (2 cycles * 500ms delays + overhead)
		Task.Delay(4000).Wait();

		// Verify results
		App.Tap("VerifyButton");

		// Check the result - should show PASSED
		var resultLabel = App.WaitForElement("ResultLabel");
		var resultText = resultLabel.GetText();
		var statusLabel = App.WaitForElement("StatusLabel");
		var statusText = statusLabel.GetText();

		TestContext.WriteLine($"Status: {statusText}");
		TestContext.WriteLine($"Result: {resultText}");

		Assert.That(resultText, Does.Contain("PASSED"),
			$"Test failed. Status: {statusText}, Result: {resultText}");
	}
}
#endif
