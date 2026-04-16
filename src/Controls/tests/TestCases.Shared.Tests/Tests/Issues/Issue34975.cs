using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34975 : _IssuesUITest
{
	public override string Issue => "[iOS] Title view memory leak when using Shell.TitleView and x:Name";

	public Issue34975(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellTitleViewWithXNameShouldNotLeakMemory()
	{
		// Navigate to the second page and use the native back button to reproduce
		// the iOS retain cycle that occurs when Shell.TitleView is combined with x:Name.
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		// Wait for the second page to load, then tap the native back arrow.
		// Using the native back navigation (not GoToAsync("..")) is required to
		// trigger the UINavigationController code path that causes the retain cycle.
		App.WaitForElement("SecondPageLabel");
		App.TapBackArrow();

		// Navigate a second time to make the leak more detectable before GC check.
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		App.WaitForElement("SecondPageLabel");
		App.TapBackArrow();

		App.WaitForElement("CheckMemoryButton");
		App.Tap("CheckMemoryButton");

		Assert.IsTrue(
			App.WaitForTextToBePresentInElement("StatusLabel", "Success", timeout: TimeSpan.FromSeconds(15)),
			"Expected page to be garbage collected but memory leak was detected.");
	}
}
