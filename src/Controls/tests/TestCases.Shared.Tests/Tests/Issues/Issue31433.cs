using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31433 : _IssuesUITest
{
	public Issue31433(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "HorizontalOptions for content inside ScrollView does not work on Android";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ContentHorizontalOptionsShouldWorkInsideScrollView()
	{
		App.WaitForElement("HStackButton1");
		VerifyScreenshot();
	}
}
