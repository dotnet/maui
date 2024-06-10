#if !WINDOWS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21609 : _IssuesUITest
{
	public Issue21609(TestDevice device) : base(device) { }

	public override string Issue => "Changing the dimensions of the CarouselView doesn't update Item Dimensions";

	[Test]
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

		ClassicAssert.AreEqual(imageInitial, imageAfterSizeChangedBacktoInitial);
		ClassicAssert.AreNotEqual(imageInitial, imageAfterSizeChange);
	}
}
#endif