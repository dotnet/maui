#if TEST_FAILS_ON_WINDOWS
//The WidthRequest property of CarouselView is not functioning correctly on Windows. Issue Link: https://github.com/dotnet/maui/issues/27680
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21609 : _IssuesUITest
{
	public Issue21609(TestDevice device) : base(device) { }

	public override string Issue => "Changing the dimensions of the CarouselView doesn't update Item Dimensions";

	[Fact]
	[Trait("Category", UITestCategories.CarouselView)]
	public void ChangingDimensionsOfCarouselViewDoesntUpdateItemDimensions()
	{
		// This test is currently not passing on windows
		// the 3rd size doesn't match the first one

		App.WaitForElement("ChangeCarouselViewDimensions");
		var imageInitial = App.WaitForElement("DotnetBot").GetRect();
		App.Tap("ChangeCarouselViewDimensions");
		var imageAfterSizeChange = App.WaitForElement("DotnetBot").GetRect();
		App.Tap("ChangeCarouselViewDimensions");
		var imageAfterSizeChangedBacktoInitial = App.WaitForElement("DotnetBot").GetRect();

		Assert.Equal(imageInitial, imageAfterSizeChangedBacktoInitial);
		Assert.NotEqual(imageInitial, imageAfterSizeChange);
	}
}
#endif