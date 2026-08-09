#if ANDROID // The fix is available on Android only. iOS and macCatalyst are tracked separately in https://github.com/dotnet/maui/issues/34693. A fix for Windows is available in an open PR (https://github.com/dotnet/maui/pull/29441), so the test is restricted on Windows, iOS and macCatalyst for now.
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