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
		[Category(UITestCategories.DatePicker)]
		public void DatePickerFocusedAndUnfocusedEventsShouldFire()
		{
			_ = App.WaitForElement("datePicker");

			App.Click("datePicker");
			App.WaitForElement("Cancel");
			App.Click("Cancel");
			App.WaitForElement("focusedLabel");
			var focusedLabelText = App.FindElement("focusedLabel").GetText();
			var unfocusedLabelText = App.FindElement("unfocusedLabel").GetText();

			Assert.That(focusedLabelText, Is.EqualTo("Focused: true"));
			Assert.That(unfocusedLabelText, Is.EqualTo("Unfocused: true"));
		}
	}
}
#endif