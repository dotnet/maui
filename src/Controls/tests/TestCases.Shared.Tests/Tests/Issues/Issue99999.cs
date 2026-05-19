using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue99999 : _IssuesUITest
{
	public override string Issue => "Shell CoordinatorLayout 0x0 after repeated GoToAsync navigation cycles on Android";

	public Issue99999(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellPageHasLayoutAfterRepeatedNavigationCycles()
	{
		// Wait for the main page
		App.WaitForElement("RunCyclesButton");

		// Run 20 rapid push/pop cycles
		App.Tap("RunCyclesButton");

		// Wait for cycles to complete — poll status label for up to 60 seconds
		bool cyclesComplete = false;
		string statusText = "";
		for (int i = 0; i < 30; i++)
		{
			Thread.Sleep(2000);
			try
			{
				var el = App.FindElement("StatusLabel");
				statusText = el.GetText() ?? "";
				if (statusText == "CyclesDone" || statusText.StartsWith("ERROR"))
				{
					cyclesComplete = true;
					break;
				}
			}
			catch
			{
				// Element might not be accessible during rapid navigation
			}
		}

		Assert.That(cyclesComplete, Is.True,
			$"Cycles did not complete within timeout. Last status: {statusText}");
		Assert.That(statusText, Is.EqualTo("CyclesDone"),
			$"Cycles should complete successfully but got: {statusText}");

		// Navigate to a fresh page - this is where the bug manifests
		App.Tap("NavigateButton");

		// Wait for the final page to appear
		App.WaitForElement("FinalPageSizeLabel", timeout: TimeSpan.FromSeconds(15));

		// Wait for size to be reported (Loaded handler waits 3s then updates label)
		Thread.Sleep(5000);

		// Check the reported page dimensions
		var sizeText = App.FindElement("FinalPageSizeLabel").GetText() ?? "null";

		// The bug causes Width=-1, Height=-1 (never laid out)
		Assert.That(sizeText, Does.Not.Contain("-1"),
			$"Page should have valid dimensions after rapid navigation cycles but got: {sizeText}");
		Assert.That(sizeText, Does.Not.Contain("0x0"),
			$"Page should not have 0x0 dimensions but got: {sizeText}");
		Assert.That(sizeText, Does.Not.Contain("waiting"),
			$"Page size should have been reported but label still says: {sizeText}");

		// Also verify the element has a non-zero rect via Appium
		var rect = App.FindElement("FinalPageSizeLabel").GetRect();
		Assert.That(rect.Width, Is.GreaterThan(0),
			"Final page label width should be > 0");
		Assert.That(rect.Height, Is.GreaterThan(0),
			"Final page label height should be > 0");
	}
}
