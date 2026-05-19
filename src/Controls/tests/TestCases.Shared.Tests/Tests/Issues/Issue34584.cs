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
		App.WaitForElement("Entry");

		App.Tap("Entry");
		App.EnterText("Entry", "test");

		App.Tap("NavigateButton");

		App.WaitForElement("TargetLabel");

		var rect = App.FindElement("TargetLabel").GetRect();

		Assert.That(rect.Y, Is.GreaterThan(50),
			"TargetLabel should not render under the status bar");
	}
}
#endif
