#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/26148
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35390 : _IssuesUITest
{
	public override string Issue => "[Android] The flyout icon tint is inconsistent across navigation when Shell.ForegroundColor is set";

	public Issue35390(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutIconShouldRespectForegroundColorAfterNavigation()
	{
		App.WaitForElement("Issue35390GoToSubPage");
		App.Tap("Issue35390GoToSubPage");

		App.WaitForElement("Issue35390SetIcon");
		App.Tap("Issue35390SetIcon");

		App.WaitForElement("Issue35390GoBack");
		App.Tap("Issue35390GoBack");

		App.WaitForElement("Issue35390Label");
		VerifyScreenshot();
	}
}
#endif
