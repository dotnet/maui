#if WINDOWS
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28130_Flyout : _IssuesUITest
	{
		public Issue28130_Flyout(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Windows] Flyout Menu Icon disappears from Window Title Bar after Navigation";

		[Fact]
		[Category(UITestCategories.FlyoutPage)]
		public void FlyoutMenuShouldNotDisappearAfterNavigation_FlyoutPage()
		{
			App.WaitForElement("detailLabel");
			App.TapInFlyoutPageFlyout("NavigateButton");
			App.WaitForElement("newPageLabel");
			App.TapFlyoutPageIcon();
		}
	}
}
#endif