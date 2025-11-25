using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6839 : _IssuesUITest
{
	public override string Issue => "Disabled CheckBox on iOS always Tints Grey";

	public Issue6839(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void DisabledCheckBoxShouldPreserveColor()
	{
		// Wait for the page to load
		App.WaitForElement("DisabledCheckBox");

		// Take a screenshot to verify the disabled checkbox preserves the purple color
		// The visual verification will show that the checkbox is purple, not grey
		VerifyScreenshot();
	}
}
