using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellFlyoutSizing : _IssuesUITest
{
	public ShellFlyoutSizing(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Width and Height";

	// TODO: IN the HostApp UI a line was commented out for the display density, need to reenable that first
	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void FlyoutHeightAndWidthResetsBackToOriginalSize()
	//{
	//	App.WaitForElement("PageLoaded");
	//	this.ShowFlyout();
	//	var initialWidth = App.WaitForElement("FlyoutHeader")[0].Rect.Width;
	//	var initialHeight = App.WaitForElement("FlyoutFooter")[0].Rect.Y;
	//	TapInFlyout("ChangeFlyoutSizes", makeSureFlyoutStaysOpen: true);
	//	Assert.AreNotEqual(initialWidth, App.WaitForElement("FlyoutHeader")[0].Rect.Width);
	//	Assert.AreNotEqual(initialHeight, App.WaitForElement("FlyoutFooter")[0].Rect.Y);
	//	TapInFlyout("ResetFlyoutSizes", makeSureFlyoutStaysOpen: true);
	//	Assert.AreEqual(initialWidth, App.WaitForElement("FlyoutHeader")[0].Rect.Width);
	//	Assert.AreEqual(initialHeight, App.WaitForElement("FlyoutFooter")[0].Rect.Y);
	//}

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void FlyoutHeightAndWidthIncreaseAndDecreaseCorrectly()
	//{
	//	App.WaitForElement("PageLoaded");
	//	this.ShowFlyout();
	//	TapInFlyout("ChangeFlyoutSizes", makeSureFlyoutStaysOpen: true);
	//	var initialWidth = App.WaitForElement("FlyoutHeader")[0].Rect.Width;
	//	var initialHeight = App.WaitForElement("FlyoutFooter")[0].Rect.Y;
	//	TapInFlyout("DecreaseFlyoutSizes", makeSureFlyoutStaysOpen: true);
	//	var newWidth = App.WaitForElement("FlyoutHeader")[0].Rect.Width;
	//	var newHeight = App.WaitForElement("FlyoutFooter")[0].Rect.Y;

	//	Assert.That(initialWidth - newWidth, Is.EqualTo(10).Within(1));
	//	Assert.That(initialHeight - newHeight, Is.EqualTo(10).Within(1));
	//}
}