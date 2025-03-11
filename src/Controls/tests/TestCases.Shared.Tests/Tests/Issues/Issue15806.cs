using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue15806 : _IssuesUITest
{
	public Issue15806(TestDevice device) : base(device) { }

	public override string Issue => "RadioButton Border color not working for focused visual state";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void ValidateRadioButtonBorderColor()
	{
		App.WaitForElement("FocusedRadioButton");
		App.Tap("NormalRadioButton");
		VerifyScreenshot();
	}
}

