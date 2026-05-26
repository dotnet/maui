#if TEST_FAILS_ON_WINDOWS // https://github.com/dotnet/maui/issues/29245 - test fails on windows due to appium related issues in carouselview
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31148 : _IssuesUITest
{
	public Issue31148(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] Items are not updated properly in CarouselView2";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselViewItemsShouldUpdateProperlyOnSourceUpdateWithPageNavigation()
	{
		App.WaitForElement("NavigateToButton");
		App.Tap("NavigateToButton");
		App.WaitForElement("ReplaceItemAndNavigateBackButton");
		App.Tap("ReplaceItemAndNavigateBackButton");
		App.WaitForElement("Replaced Item");
	}
}
#endif