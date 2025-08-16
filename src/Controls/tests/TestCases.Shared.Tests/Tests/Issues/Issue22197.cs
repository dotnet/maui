using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22197 : _IssuesUITest
{
	public Issue22197(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "LineHeight with HTML Label not working";

	[Test]
	[Category(UITestCategories.Label)]
	public void LineHeightWithHTMLLabelShouldWork()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}