using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35587 : _IssuesUITest
{
	public Issue35587(TestDevice device) : base(device) { }

	public override string Issue => "RadioButton BorderColor and BorderWidth not applied when dynamically updated at runtime";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButtonBorderAppliedAtRuntime()
	{
		App.WaitForElement("TestRadioButton");

		// Tap the button to dynamically apply border properties
		App.Tap("ApplyBorderButton");

		// Verify the border is visually applied
		VerifyScreenshot();
	}
}
