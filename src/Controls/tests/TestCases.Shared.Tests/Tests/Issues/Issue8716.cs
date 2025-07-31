#if TEST_FAILS_ON_WINDOWS // A fix for this issue is already available for Windows platform in an open PR (https://github.com/dotnet/maui/pull/29441), so the test is restricted on Windows for now. 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8716 : _IssuesUITest
{
	public Issue8716(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[Shell][Android] The truth is out there...but not on top tab search handlers";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShouldUpdateSearchViewOnPageNavigation()
	{
		App.WaitForElement("MainPageButton");
		App.TapTab("DogsPage");

		VerifyScreenshot();
	}
}
#endif