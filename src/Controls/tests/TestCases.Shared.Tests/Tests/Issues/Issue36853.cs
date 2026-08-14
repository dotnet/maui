using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36853 : _IssuesUITest
{
	public override string Issue => "Shell singleton page renders blank when re-pushed after absolute route PopToRoot on Android";

	public Issue36853(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void SingletonPageShouldRenderAfterPopToRootAndRePush()
	{
		// Step 1: From root, push SecondPage (singleton)
		App.WaitForElement("Issue36853GoToSecond");
		App.Tap("Issue36853GoToSecond");

		// Step 2: Verify SecondPage renders
		App.WaitForElement("Issue36853SecondLabel");

		// Step 3: Push ThirdPage on top (so stack is Root → Second → Third)
		App.Tap("Issue36853GoToThird");
		App.WaitForElement("Issue36853ThirdLabel");

		// Step 4: PopToRoot via absolute route ///
		App.Tap("Issue36853ResetToRoot");
		App.WaitForElement("Issue36853MainLabel");

		// Step 5: Re-push SecondPage (same singleton instance)
		App.Tap("Issue36853GoToSecond");

		// Step 6: SecondPage content must be visible — not blank
		// Without the fix, WaitForElement will timeout because the stale fragment
		// has a disconnected handler and OnCreateView never fires — page is blank.
		App.WaitForElement("Issue36853SecondLabel");
	}
}
