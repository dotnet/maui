#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38234TabbedPageTest : _IssuesUITest
{
	public override string Issue => "Android Material 3 TabbedPage selection color does not update with AppTheme";

	public Issue38234TabbedPageTest(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void TabbedPageSelectionColorUpdatesAfterThemeChange()
	{
		App.WaitForElement("Issue38234TabbedPage_SwitchToDarkThemeButton");
		App.Tap("Issue38234TabbedPage_SwitchToDarkThemeButton");
		App.WaitForElement("Dark theme active");

		VerifyScreenshot();
	}
}
#endif