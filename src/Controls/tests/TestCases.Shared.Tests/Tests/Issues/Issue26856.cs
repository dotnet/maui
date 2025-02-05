#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26856 : _IssuesUITest
	{
		public override string Issue => "MenuFlyoutItems programmatically added to MenuFlyoutSubItems are not visible";

		public Issue26856(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Page)]
		public void MenuFlyoutItemShouldVisibleInsideMenuFlyoutSubItems()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			App.Tap("Menu Flyout Item");
			App.WaitForElement("Flyout");
			App.Tap("Flyout");
			VerifyScreenshot();
		}
	}
}
#endif
