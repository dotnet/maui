using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30771 : _IssuesUITest
{
	public Issue30771(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "SearchHandler overlaps title and title view";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerShouldNotOverlap()
	{
		App.EnterText("Search here", "Test Search");
		App.EnterText("SearchEntry", "Test Search");
		VerifyScreenshot();
	}
}
