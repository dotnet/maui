#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25679 : _IssuesUITest
{
	public Issue25679(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "When using TabbedPage TabBar is not visible";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageRendersOnCatalyst()
	{
		App.WaitForElement("Issue25679Label");
		VerifyScreenshot();
	}
}
#endif