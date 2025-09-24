#if MACCATALYST || WINDOWS // Maximizing the FlyoutPage window is applicable only on the MacCatalyst and Windows platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22719 : _IssuesUITest
{
	public Issue22719(TestDevice device) : base(device)
	{ }

	public override string Issue => "FlyoutPage flyout disappears when maximizing the window on the mac platform";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void ShouldFlyoutBeVisibleAfterMaximizingWindow()
	{
		App.WaitForElement("Label");
#if MACCATALYST // The EnterFullScreen script is applicable only for MacCatalyst
		App.EnterFullScreen();
#endif
		VerifyScreenshot();
	}
}
#endif