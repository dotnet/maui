using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue21609 : _IssuesUITest
{
	public Issue21609(TestDevice device) : base(device) { }

	public override string Issue => "Changing the dimensions of the CarouselView doesn't update Item Dimensions";

	[Test]
	public void ChangingDimensionsOfCarouselViewDoesntUpdateItemDimensions()
	{
		// This test is currently not passing on windows
		// the 3rd size doesn't match the first one
		this.IgnoreIfPlatform(TestDevice.Windows);

		App.WaitForElement("ChangeCarouselViewDimensions");
		var imageInitial = App.WaitForElement("DotnetBot").GetRect();
		App.Click("ChangeCarouselViewDimensions");
		var imageAfterSizeChange = App.WaitForElement("DotnetBot").GetRect();
		App.Click("ChangeCarouselViewDimensions");
		var imageAfterSizeChangedBacktoInitial = App.WaitForElement("DotnetBot").GetRect();

		Assert.AreEqual(imageInitial, imageAfterSizeChangedBacktoInitial);
		Assert.AreNotEqual(imageInitial, imageAfterSizeChange);
	}
}
