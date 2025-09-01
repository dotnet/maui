using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31433 : _IssuesUITest
{
	public Issue31433(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "HorizontalOptions for FlexLayout does not work";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void HorizontalOptionsForFlexLayoutShouldWork()
	{
		App.WaitForElement("button");
		VerifyScreenshot();
	}
}