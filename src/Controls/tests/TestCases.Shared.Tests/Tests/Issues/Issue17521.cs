using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17521 : _IssuesUITest
{
	public Issue17521(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Shell.SearchHandler visible in details page on Windows 11";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShouldUpdateSearchHandlerOnPageNavigation()
	{
		App.WaitForElement("MainButton");
		App.Tap("MainButton");
		VerifyScreenshot();
	}
}
