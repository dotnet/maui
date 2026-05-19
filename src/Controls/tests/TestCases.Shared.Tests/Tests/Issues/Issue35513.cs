using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35513 : _IssuesUITest
{
	public Issue35513(TestDevice device) : base(device) { }

	public override string Issue => "[Windows, Android, iOS & Mac] Button TextColor does not restore to platform default when reset to null after dynamic update";

	[Test, Order(1)]
	[Category(UITestCategories.Button)]
	public void ButtonTextColorUpdatesToDarkRed()
	{
		App.WaitForElement("SampleButton");

		// Set text color to DarkRed
		App.Tap("SetTextColorButton");
		VerifyScreenshot("AfterSetTextColorDarkRed");
	}

	[Test, Order(2)]
	[Category(UITestCategories.Button)]
	public void ButtonTextColorRestoresToDefaultAfterResetToNull()
	{
		App.WaitForElement("SampleButton");

		// Set text color to DarkRed first, then reset to verify default restoration.
		App.Tap("SetTextColorButton");

		// Reset text color to null (should restore platform default)
		App.Tap("ResetTextColorButton");
		VerifyScreenshot("AfterResetTextColorToNull");
	}
}
