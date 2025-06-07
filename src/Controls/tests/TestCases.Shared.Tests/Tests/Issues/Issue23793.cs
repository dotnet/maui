using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23793(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "When the date is changed, the DatePicker (Format='D') text didn't keep format"; 

		[Test]
		[Category(UITestCategories.DatePicker)]
		public void DatePickerTextShouldHaveCorrectFormat()
		{
			App.WaitForElement("Button");
			App.Click("Button");
			App.WaitForTextToBePresentInElement("DatePicker","Monday, August 19, 2024");
		}
	}
}