using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14200 : _IssuesUITest
{
	public Issue14200(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Vertical Stack Layout compressed in IOS and MacCatalyst";

	[Test]
	[Category(UITestCategories.Layout)]
	public void ShouldShowtheTextFully()
	{
		App.WaitForElement("Issue14200Label");
		VerifyScreenshot();
	}
}
