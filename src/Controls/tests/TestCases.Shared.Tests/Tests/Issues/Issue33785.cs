#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue33785 : _IssuesUITest
{
	public Issue33785(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] FlyoutPage CollapsedPaneWidth Not Working";
	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPageCollapsedPaneWidth()
	{
		App.WaitForElement("CollapsedPaneLabel");
		App.TapFlyoutPageIcon();
		App.Tap("FlyoutItem");
		App.TapFlyoutPageIcon();
		VerifyScreenshot();
	}
}
#endif