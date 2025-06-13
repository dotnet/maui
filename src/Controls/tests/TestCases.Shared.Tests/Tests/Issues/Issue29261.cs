#if TEST_FAILS_ON_CATALYST
// On Catalyst, Swipe actions not supported in Appium.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29261 : _IssuesUITest
{
	public override string Issue => "CarouselViewHandler2 for iOS does not properly bounce back when reaching the end with Loop=false";

	public Issue29261(TestDevice device)
	: base(device)
	{ }

	[Fact]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewBounceTest()
	{
		App.WaitForElement("carouselview");
		App.SwipeRightToLeft("carouselview");
		App.SwipeRightToLeft("carouselview");
		App.SwipeRightToLeft("carouselview");
		var text = App.FindElement("positionLabel").GetText();
		Assert.Equal("CarouselView Position - 2", text);
	}
}
#endif