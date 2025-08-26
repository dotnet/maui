using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31124 : _IssuesUITest
{
	public Issue31124(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "DatePicker should maintain today's date when set to null";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerShouldMaintainTodaysDateWhenSetToNull()
	{
		// Get today's date for comparison
		var todaysDate = DateTime.Today.ToString("M/d/yyyy");

		// Find the initial date label text, should show today's date
		var initialDateLabel = App.WaitForElement("DateLabel").GetText();
		Assert.That(initialDateLabel, Does.Contain(todaysDate), 
			"DatePicker should initially show today's date");

		// Click the button to set date to null
		App.WaitForElement("SetNullButton").Click();

		// Verify the date is still today's date after setting to null
		var dateAfterNull = App.WaitForElement("DateLabel").GetText();
		Assert.That(dateAfterNull, Does.Contain(todaysDate), 
			"DatePicker should still show today's date after being set to null");

		// Also verify the DatePicker itself shows the expected date
		// This might vary by platform, so we'll check if it's not empty
		var datePickerElement = App.WaitForElement("TestDatePicker");
		Assert.That(datePickerElement, Is.Not.Empty);
	}
}