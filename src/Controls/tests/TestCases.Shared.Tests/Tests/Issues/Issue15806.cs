#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // RadioButton Focused/Unfocused events not firing on Android and iOS, Issue Link: https://github.com/dotnet/maui/issues/28163
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue15806 : _IssuesUITest
{
	public Issue15806(TestDevice device) : base(device) { }

	public override string Issue => "RadioButton Border color not working for focused visual state";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void ValidateRadioButtonBorderColor()
	{
		App.WaitForElement("FocusedRadioButton");
		App.Tap("NormalRadioButton");
		VerifyScreenshot();
	}
}
#endif
