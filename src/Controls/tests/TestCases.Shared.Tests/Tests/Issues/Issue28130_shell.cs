#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28130_Shell : _IssuesUITest
	{
		public Issue28130_Shell(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Windows] Shell Flyout Menu Icon disappears from Window Title Bar after Navigation";

		[Test]
		[Category(UITestCategories.Shell)]
		public void FlyoutMenuShouldNotDisappearAfterNavigation_Shell()
		{
			App.WaitForElement("detailLabel");
			App.TapShellFlyoutIcon();
			App.Tap("Navigate to Page 2");
			App.WaitForElement("newPageLabel");
			App.TapShellFlyoutIcon();
		}
	}
}
#endif