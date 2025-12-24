using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30254 : _IssuesUITest
{
	public override string Issue => "(Windows) Shell.FlyoutBehavior=Flyout forces the title height space above the tab bar even if the page title is empty";

	public Issue30254(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShouldHideHeaderWhenTitleEmpty()
	{
		App.WaitForElement("MainPageButton");
		VerifyScreenshot();
	}
}