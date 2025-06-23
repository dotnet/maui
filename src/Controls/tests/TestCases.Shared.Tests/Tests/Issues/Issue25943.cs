#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //VerifyScreenShot() is not implemented on Mac, and the DatePicker UI behaves differently on iOS and doesn't have a DatePickerDialog like on other platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25943 : _IssuesUITest
	{
		public Issue25943(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] DatePicker Graphical Bug";

		[Test]
		[Category(UITestCategories.DatePicker)]
		public void DatePickerShouldDisplayProperSelectedDate()
		{
			App.WaitForElement("DatePicker");
			App.Tap("DatePicker");
#if ANDROID
			var nextMonthButton = App.WaitForElement(AppiumQuery.ByXPath("//android.widget.ImageButton[@content-desc='Next month']"), timeout: TimeSpan.FromSeconds(2));
			nextMonthButton.Tap();
#elif WINDOWS
            App.Tap("NextButton");
#endif
			VerifyScreenshot();
		}
	}
}
#endif