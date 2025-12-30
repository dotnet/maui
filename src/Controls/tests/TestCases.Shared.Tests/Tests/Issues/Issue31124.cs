using System.Text.RegularExpressions;
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
		// Find the initial date label text, should show today's date
		var initialDateLabel = App.WaitForElement("DateLabel").GetText();
		Assert.That(ContainsValidDate(initialDateLabel), Is.True, 
			"DatePicker should initially show today's date");

		// Click the button to set date to null
		App.WaitForElement("SetNullButton").Click();

		// Verify the date is empty after setting to null
		var dateAfterNull = App.WaitForElement("DateLabel").GetText();
		Assert.That(ContainsValidDate(dateAfterNull), Is.False,
			"DatePicker should be empty after being set to null");
	}
	
	static bool IsValidDate(string? input)
	{
		return !string.IsNullOrWhiteSpace(input) && DateTime.TryParse(input, out _);
	}

	static bool ContainsValidDate(string? labelText)
	{
		if (string.IsNullOrWhiteSpace(labelText))
			return false;
    
		// Extract the date part after "Current Date: "
		var prefix = "Current Date: ";
		if (labelText.StartsWith(prefix))
		{
			var datePart = labelText.Substring(prefix.Length);
			return IsValidDate(datePart);
		}
    
		return false;
	}
}