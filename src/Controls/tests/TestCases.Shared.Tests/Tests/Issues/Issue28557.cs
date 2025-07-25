using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28557 : _IssuesUITest
{
	public Issue28557(TestDevice device) : base(device) { }

	public override string Issue => "NRE in CarouselViewController on iOS 15.5 & 16.4";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselViewShouldNotCrashOnSourceUpdateWithPageNavigation()
	{
		App.WaitForElement("TestCarouselView");
		App.Tap("NavigateToButton");
		App.WaitForElement("SourceUpdateAndNavigateBackButton");
		App.Tap("SourceUpdateAndNavigateBackButton");
		App.WaitForElement("TestCarouselView");
	}
}