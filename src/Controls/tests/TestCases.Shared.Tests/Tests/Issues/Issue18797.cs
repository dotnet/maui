#if ANDROID
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18797 : _IssuesUITest
	{
		public override string Issue => "[Android] Datepicker focus and unfocus event not firing on android";

		public Issue18797(TestDevice device) : base(device)
		{
		}

		[Test]
		public void DatePickerFocusedAndUnfocusedEventsShouldFire()
		{
			_ = App.WaitForElement("datePicker");

			App.Click("datePicker");

			((AppiumApp)App).Driver.FindElements(MobileBy.XPath("//android.widget.Button[@resource-id=\"android:id/button1\"]")).First().Click();

			var focusedLabelText = App.FindElement("focusedLabel").GetText();
			var unfocusedLabelText = App.FindElement("unfocusedLabel").GetText();

			ClassicAssert.True(focusedLabelText == "Focused: true");
			ClassicAssert.True(unfocusedLabelText == "Unfocused: true");
		}
	}
}
#endif