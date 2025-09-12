using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25921 : _IssuesUITest
{
	public override string Issue => "[Windows] Setting BackgroundColor for Slider updates the Maximum Track Color";

	public Issue25921(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Slider)]
	public void VerifySliderColors()
	{
		App.WaitForElement("ColorChangeButton");
		App.Tap("ColorChangeButton");
		VerifyScreenshot();
	}
}