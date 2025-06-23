using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29693 : _IssuesUITest
{
	public override string Issue => "The default native on color is not displayed when the Switch on color is not explicitly set";

	public Issue29693(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Switch)]
	public void VerifySwitchDefaultColors()
	{
		App.WaitForElement("button1");
		App.Tap("button1");
		App.Tap("button2");
		VerifyScreenshot();
	}
}