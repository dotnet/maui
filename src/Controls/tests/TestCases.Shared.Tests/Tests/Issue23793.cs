using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
	public class Issue23793(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "DatePicker Format property is not working on windows"; 

		[Test]
		[Category(UITestCategories.DatePicker)]
		public void DatePickerTextShouldHaveCorrectFormat()
		{
			App.WaitForElement("Button");
			App.Click("Button");
			App.WaitForTextToBePresentInElement("DatePicker","Saturday, June 14, 2025");
		}
	}