 #if ANDROID // Scrolling cannot be reliably performed in iOS UI tests for this scenario, and the issue occurs only on Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33703 : _IssuesUITest
{
	public override string Issue => "Empty space above TabBar after navigating back when TabBar visibility is toggled";

	public Issue33703(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarShouldNotHaveEmptySpaceAfterNavigatingBack()
	{
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");
		App.WaitForElement("DetailLabel");
		this.Back();
		App.WaitForElement("NavigateButton");
		App.ScrollTo("BottomLabel");
		App.WaitForElement("BottomLabel");
		VerifyScreenshot();
	}
}
#endif
