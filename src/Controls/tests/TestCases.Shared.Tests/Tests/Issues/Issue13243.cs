using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue13243 : _IssuesUITest
{
	public Issue13243(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Flyout Not Displayed on Android When FlyoutWidth Is Set Only for Desktop via OnIdiom";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShouldDisplayFlyoutWhenOnIdiomHasNoDefaultValue()
	{
		App.WaitForElement("Label");
		App.TapShellFlyoutIcon();
		App.WaitForElement("Page2");
	}
}