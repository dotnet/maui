#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34584 : _IssuesUITest
{
	public Issue34584(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Content renders under status bar when navigating with keyboard open to a page with NavBarIsVisible=False";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ContentShouldNotRenderUnderStatusBarAfterNavigatingWithKeyboardOpen()
	{
		// Wait for the main page
		App.WaitForElement("Entry");

		// Open keyboard
		App.Tap("Entry");
		App.EnterText("Entry", "test");

		// Navigate while keyboard is open
		App.PressEnter();

		// Wait for destination page
		App.WaitForElement("TargetLabel");

		// Poll for a short period to detect transient layout issues during navigation
		bool foundInvalidPosition = false;

		var start = DateTime.UtcNow;
		var timeout = TimeSpan.FromSeconds(1);

		while (DateTime.UtcNow - start < timeout)
		{
			var rect = App.FindElement("TargetLabel").GetRect();

			// Small threshold to account for device differences
			if (rect.Y < 5)
			{
				foundInvalidPosition = true;
				break;
			}

			System.Threading.Thread.Sleep(50);
		}

		Assert.That(foundInvalidPosition, Is.False,
			"TargetLabel was temporarily rendered under the status bar");
	}
}
#endif