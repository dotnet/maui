using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17414 : _IssuesUITest
{
	public override string Issue => "Default styling for controls does not work";

	public Issue17414(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	[Category(UITestCategories.Shell)]
	public void VerifyFlyoutContentHasNoDefaultCornerRadius()
	{
		App.WaitForElement("MainPageLabel");
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}
}