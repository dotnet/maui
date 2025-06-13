#if TEST_FAILS_ON_WINDOWS

using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28930 : _IssuesUITest
{
	public Issue28930(TestDevice device) : base(device) { }

	public override string Issue => "Incorrect label LineBreakMode in IOS inside CarouselView";

	[Fact]
	[Category(UITestCategories.CarouselView)]
	public void LineBreakModeInCarouselViewShouldWork()
	{
		App.WaitForElement("dotnetbot1");
		App.WaitForElement("dotnetbot2");
		App.WaitForElement("dotnetbot3");
		App.WaitForElement("dotnetbot4");

		App.ScrollRight("MyCarousel", swipePercentage: 0.9, swipeSpeed: 200);

		Assert.Equal("Item 2", App.WaitForElementTillPageNavigationSettled("Item 2").GetText());
	}
}

#endif