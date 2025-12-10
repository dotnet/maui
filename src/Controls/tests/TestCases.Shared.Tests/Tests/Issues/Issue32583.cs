using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32583 : _IssuesUITest
{
	public Issue32583(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell Navigation Bar Remains Visible After Navigating Back to a ContentPage with a Hidden Navigation Bar";

	[Test]
	[Category(UITestCategories.Shell)]
	public void NavigationBarShouldRemainHiddenAfterNavigatingBack()
	{
		App.WaitForElement("Issue32583NavigateButton");
		App.Tap("Issue32583NavigateButton");
		
		App.WaitForElement("Issue32583BackButton");
		App.Tap("Issue32583BackButton");

		App.WaitForElement("Issue32583DescriptionLabel");
		VerifyScreenshot();
	}
}