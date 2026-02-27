using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28536 : _IssuesUITest
{
	const string Switch = "Switch";
	public override string Issue => "Navigation breaks VisualState styles for Picker";

	public Issue28536(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Picker)]
	public void CheckNavigationBreaksVisualStatesForPicker()
	{
		App.WaitForElement("VisualStatePicker");
		App.Tap(Switch);
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");
		App.WaitForElement("Next Page");
		App.TapBackArrow();
		App.WaitForElement(Switch).Tap();
		VerifyScreenshot();
	}
}
