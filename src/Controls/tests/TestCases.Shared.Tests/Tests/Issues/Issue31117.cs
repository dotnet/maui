#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31117 : _IssuesUITest
{
	public Issue31117(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "DatePicker initially shows year in 4 digits, but after changing the year it displays only 2 digits in net 10.0";

	[Test, Order(1)]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerYearFormat_InitialState_DisplaysFourDigitYear()
	{
		// Verify initial display shows 4-digit year for 2024-12-24
		var datePickerText = App.WaitForElement("MyDatePicker").GetText();
		Assert.That(datePickerText, Does.Contain("2024"), "DatePicker should display 4-digit year initially");
		Assert.That(datePickerText, Does.Not.Contain("/24"), "DatePicker should not show 2-digit year pattern");

		var displayLabelText = App.WaitForElement("DisplayLabel").GetText();
		Assert.That(displayLabelText, Does.Contain("2024"), "Display label should show 4-digit year");
	}

	[Test, Order(2)]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerYearFormat_UserInteraction_MaintainsFourDigitYear()
	{
		// Get initial text
		var initialText = App.WaitForElement("MyDatePicker").GetText();
		Assert.That(initialText, Does.Contain("2024"), "Initial display should show 4-digit year");

		// Simulate user interaction by opening and closing the DatePicker
		App.Tap("MyDatePicker");
		
		// On iOS, tap Done to close the picker
		App.WaitForElement("Done");
		App.Tap("Done");

		// Verify text still shows 4-digit year after interaction
		var afterInteractionText = App.WaitForElement("MyDatePicker").GetText();
		Assert.That(afterInteractionText, Does.Contain("2024"), "After interaction should still show 4-digit year");
		Assert.That(afterInteractionText, Does.Not.Contain("/24"),
			"After interaction should not show 2-digit year pattern");

		// Ensure format consistency
		Assert.That(afterInteractionText, Is.EqualTo(initialText),
			"Date format should remain consistent after user interaction");
	}

	[Test, Order(3)]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerYearFormat_SetDateViaEntry_DisplaysFourDigitYear()
	{
		// Set a different date using the Entry field
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "1/1/2025");
		App.Tap("SetDateButton");

		// Verify DatePicker shows 4-digit year for the new date
		var datePickerText = App.WaitForElement("MyDatePicker").GetText();
		Assert.That(datePickerText, Does.Contain("2025"), "DatePicker should display 4-digit year for set date");
		Assert.That(datePickerText, Does.Not.Contain("/25"), "DatePicker should not show 2-digit year pattern");

		var displayLabelText = App.WaitForElement("DisplayLabel").GetText();
		Assert.That(displayLabelText, Does.Contain("2025"), "Display label should show 4-digit year for set date");
	}

	[Test, Order(4)]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerYearFormat_MultipleYearValues_AllShowFourDigits()
	{
		var testYears = new[] { "2020", "2023", "2026", "2030" };
		var testDates = new[] { "1/1/2020", "2/2/2023", "3/3/2026", "4/4/2030" };

		for (int i = 0; i < testDates.Length; i++)
		{
			// Set the test date
			App.WaitForElement("DateEntry");
			App.ClearText("DateEntry");
			App.EnterText("DateEntry", testDates[i]);
			App.Tap("SetDateButton");

			// Verify 4-digit year display
			var datePickerText = App.WaitForElement("MyDatePicker").GetText();
			Assert.That(datePickerText, Does.Contain(testYears[i]),
				$"DatePicker should display 4-digit year {testYears[i]} for date {testDates[i]}");
		}
	}

	[Test, Order(5)]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerYearFormat_iOS_ConsistentAfterMultipleInteractions()
	{
		// This test specifically validates the iOS year format fix through multiple interactions

		// Set initial date
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "3/3/2024");
		App.Tap("SetDateButton");

		var initialText = App.WaitForElement("MyDatePicker").GetText();
		Assert.That(initialText, Does.Contain("2024"), "Initial state should show 4-digit year");

		// Perform multiple open/close cycles to test consistency
		for (int i = 0; i < 3; i++)
		{
			// Open picker
			App.Tap("MyDatePicker");
			App.WaitForElement("Done");

			// Close picker
			App.Tap("Done");

			// Verify format is still consistent
			var currentText = App.WaitForElement("MyDatePicker").GetText();
			Assert.That(currentText, Does.Contain("2024"),
				$"After interaction #{i + 1}, should still show 4-digit year");
			Assert.That(currentText, Does.Not.Contain("/24"),
				$"After interaction #{i + 1}, should not show 2-digit year pattern");
			Assert.That(currentText, Is.EqualTo(initialText),
				$"After interaction #{i + 1}, format should remain consistent");
		}
	}
}
#endif