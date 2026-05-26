using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29325 : _IssuesUITest
{
	public Issue29325(TestDevice device) : base(device) { }

	public override string Issue => "Button Shadow Color Transparency Not Applied Correctly";

	[Test]
	[Category(UITestCategories.Button)]
	public void ShouldUpdateButtonShadowWithTransparentColor()
	{
		App.WaitForElement("withoutAlphaOpacityButton");
		App.WaitForElement("alphaButton");
		App.WaitForElement("opacityButton");
		App.WaitForElement("alphaOpacityButton");
		VerifyScreenshot();
	}
}