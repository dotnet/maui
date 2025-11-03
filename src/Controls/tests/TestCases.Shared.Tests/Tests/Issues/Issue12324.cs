using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12324 : _IssuesUITest
{
	public Issue12324(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Tabbedpage should not have visual bug";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Issue12324TabbedPageVisualTest()
	{
		App.WaitForElement("Label12324");
		VerifyScreenshot();
	}
}