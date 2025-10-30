using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28570 : _IssuesUITest
{
	public Issue28570(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Setting BackButtonBehavior to not visible or not enabled does not work";

	[Test]
	[Category(UITestCategories.Shell)]
	[Category(UITestCategories.Navigation)]
	public void BackButtonShouldNotBeVisible()
	{
		App.WaitForElement("NavigateToDetailButton");
		App.Click("NavigateToDetailButton");
		App.WaitForElement("HelloLabel");
		App.WaitForNoElement("BackButton");
	}
}