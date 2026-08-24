using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35513 : _IssuesUITest
{
	public Issue35513(TestDevice device) : base(device) { }

	public override string Issue => "Button TextColor does not restore to platform default when reset to null after dynamic update";

	[Test, Order(1)]
	[Category(UITestCategories.Button)]
	public void TextViewBasedControlsTextColorUpdates()
	{
		App.WaitForElement("SetTextColorButton");
		App.Tap("SetTextColorButton");
		VerifyScreenshot("AfterSetTextColorOnTextViewBasedControls");
	}

	[Test, Order(2)]
	[Category(UITestCategories.Button)]
	public void TextViewBasedControlsRestoreDefaultAfterResetToNull()
	{
		App.WaitForElement("SetTextColorButton");
		App.Tap("SetTextColorButton");
		App.Tap("ResetTextColorButton");
		VerifyScreenshot("AfterResetTextColorToNullOnTextViewBasedControls");
	}
}
