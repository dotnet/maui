using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue28153 : _IssuesUITest
{
	public Issue28153(TestDevice device) : base(device) { }

	public override string Issue => "The border color of the RadioButton is visible in Windows only";

	[Fact]
	[Category(UITestCategories.RadioButton)]
	public void ValidateRadioButtonNoBorderColorWhenNoBorderWidth()
	{
		App.WaitForElement("RadioButtonWithoutBorder");
		VerifyScreenshot();
	}
}

