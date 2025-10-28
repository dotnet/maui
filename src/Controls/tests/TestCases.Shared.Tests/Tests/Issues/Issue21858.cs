#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21858 : _IssuesUITest
{

	public Issue21858(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "DatePicker not respecting UserAppTheme";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerShouldRespectUserAppTheme()
	{
		App.WaitForElement("ToggleThemeButton");
		App.Click("ToggleThemeButton");
		App.Click("DatePicker");
		VerifyScreenshot();
	}
}
#endif