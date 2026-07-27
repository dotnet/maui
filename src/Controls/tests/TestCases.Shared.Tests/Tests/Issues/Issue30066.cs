using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30066 : _IssuesUITest
{
	public Issue30066(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "DatePicker CharacterSpacing Property Not Working on Windows";

	[Test, Order(1)]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerCharacterSpacingShouldApply()
	{
		App.WaitForElement("TestDatePicker");

		// Take a screenshot to verify character spacing is applied
		// On Windows, the DatePicker text should show increased character spacing
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerCharacterSpacingCanChange()
	{
		App.WaitForElement("TestDatePicker");
		App.WaitForElement("ChangeSpacingButton");

		// Change the character spacing by clicking the button
		App.Tap("ChangeSpacingButton");

		// Take a screenshot to verify the character spacing changed
		VerifyScreenshot();
	}
}