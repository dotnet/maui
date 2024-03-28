using NUnit.Framework;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
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
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });
			_ = App.WaitForElement("datePicker");

			App.Click("datePicker");

			((AppiumApp)App).Driver.FindElements(MobileBy.XPath("//android.widget.Button[@resource-id=\"android:id/button1\"]")).First().Click();

			var focusedLabelText = App.FindElement("focusedLabel").GetText();
			var unfocusedLabelText = App.FindElement("unfocusedLabel").GetText();

			Assert.True(focusedLabelText == "Focused: true");
			Assert.True(unfocusedLabelText == "Unfocused: true");
		}
	}
}
