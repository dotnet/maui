#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38234 : _IssuesUITest
{
	public override string Issue => "Android Material 3 Shell TabBar selection color does not update with AppTheme";

	public Issue38234(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void ShellTabBarSelectionColorUpdatesAfterThemeChange()
	{
		App.WaitForElement("SwitchToDarkThemeButton");
		App.Tap("SwitchToDarkThemeButton");
		App.WaitForElement("Dark theme active");

		VerifyScreenshot();
	}
}
#endif