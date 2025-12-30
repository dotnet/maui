#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28811 : _IssuesUITest
{
	public Issue28811(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Overriding back button functionality with OnBackButtonPressed returning false in a modally pushed page causes stack overflow";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void OverridingBackButtonShouldNotCauseStackOverflow()
	{
		App.WaitForElement("NavigateToDetailPage");
		App.Click("NavigateToDetailPage");
		App.WaitForElement("Issue28811DetailPage");
		App.Back();
		App.WaitForElement("NavigateToDetailPage");
	}
}
#endif