using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33703 : _IssuesUITest
{
	public override string Issue => "Empty space above TabBar after navigate back when TabBar visibility toggled";

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
		App.ScrollDown("MainScrollView", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("BottomLabel");
		VerifyScreenshot();
	}
}
