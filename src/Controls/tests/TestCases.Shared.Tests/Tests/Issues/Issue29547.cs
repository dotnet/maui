using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1 : _IssuesUITest
{
	public override string Issue => "The default native colors are not displayed when the Switch on color is not explicitly set";

	public Issue1(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Switch)]
	public void VerifySwitchDefaultColors()
	{
		App.WaitForElement("switchControl");
		VerifyScreenshot();
	}
}