#if IOS || MACCATALYST // The header offset is read from the iOS platform view in the HostApp page
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellFlyoutHeaderScrollViewContent : _IssuesUITest
{
	public ShellFlyoutHeaderScrollViewContent(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell flyout header with ScrollView flyout content";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutHeaderReturnsToTopWhenScrollViewContentScrollsBack()
	{
		App.WaitForElement("RunButton");
		App.Tap("RunButton");

		// The page scrolls the flyout ScrollView down and back to the top, then reports the
		// header's platform frame offset — which must be 0 again once the content is back
		// at the top under FlyoutHeaderBehavior.Scroll.
		var success = App.WaitForTextToBePresentInElement("ResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(success, Is.True, $"Flyout header did not return to the top: {resultText}");
	}
}
#endif
