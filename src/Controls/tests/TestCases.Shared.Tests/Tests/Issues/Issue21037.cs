using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21037 : _IssuesUITest
{
	public Issue21037(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Switching to an existing page with SetTitleView used in Flyout menu causes crash on Windows";

	[Test]
	[Category(UITestCategories.TitleView)]
	public void NavigatingBetweenPagesWithSetTitleViewShouldNotCrash()
	{
		App.WaitForElement("OpenMenuButton");
		App.Tap("OpenMenuButton");
		App.WaitForElement("SecondButton");
		App.Tap("SecondButton");

		App.WaitForElement("OpenMenuButton");
		App.Tap("OpenMenuButton");
		App.WaitForElement("HomeButton");
		App.Tap("HomeButton");

		App.WaitForElement("OpenMenuButton");
		VerifyScreenshot();
	}
}
