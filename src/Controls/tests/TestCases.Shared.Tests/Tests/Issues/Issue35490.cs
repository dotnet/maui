#if IOS || MACCATALYST // The floating glass tab bar is a UIKit-only feature introduced in iOS/MacCatalyst 26. This issue does not affect Android or Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35490 : _IssuesUITest
{
	public Issue35490(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS 26] TabbedPage with NavigationPage children clips content above floating glass tab bar";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void NavigationPageChildContentExtendsUnderFloatingTabBar()
	{
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
#if IOS
		var platformVersion = ((AppiumApp)App).Driver.Capabilities.GetCapability("platformVersion") as string
			?? throw new InvalidOperationException("iOS platformVersion capability is missing.");
		if (Version.Parse(platformVersion).Major >= 26)
		{
			var content = App.FindElement(AppiumQuery.ByXPath("//XCUIElementTypeStaticText[@name='Tab1Label']/parent::*")).GetRect();
			var tabBar = App.FindElement(AppiumQuery.ByXPath("//XCUIElementTypeTabBar")).GetRect();
			Assert.That(content.Y + content.Height, Is.EqualTo(tabBar.Y + tabBar.Height),
				"NavigationPage child content must extend under the floating tab bar.");
		}
#endif
	}
}
#endif
