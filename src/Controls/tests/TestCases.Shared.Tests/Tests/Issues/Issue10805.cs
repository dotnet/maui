using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10805 : _IssuesUITest
	{
		public Issue10805(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Date Format property in DatePicker not working on Windows with date Format";

		[Test]
		[Category(UITestCategories.DatePicker)]
		public void VerifyDatePickerDateFormat()
		{
			App.WaitForElement("DatePickerLabel");
			VerifyScreenshot();
		}
	}
}