#if TEST_FAILS_ON_WINDOWS 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24878 : _IssuesUITest
{
	public Issue24878(TestDevice device) : base(device) { }

	public override string Issue => "AppThemeBinding does not work on ToolbarItems";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemsShouldBeVisible()
	{
		App.WaitForElement("button");
		VerifyScreenshot("ToolbarItemsShouldBeVisible_Before");

		App.Click("button");

		App.WaitForElement("label");
		VerifyScreenshot("ToolbarItemsShouldBeVisible_After");
	}
}
#endif