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
	//	RunningApp.WaitForElement("PageLoaded");
	//	this.ShowFlyout();
	//	var initialWidth = RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width;
	//	var initialHeight = RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y;
	//	TapInFlyout("ChangeFlyoutSizes", makeSureFlyoutStaysOpen: true);
	//	Assert.AreNotEqual(initialWidth, RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width);
	//	Assert.AreNotEqual(initialHeight, RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y);
	//	TapInFlyout("ResetFlyoutSizes", makeSureFlyoutStaysOpen: true);
	//	Assert.AreEqual(initialWidth, RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width);
	//	Assert.AreEqual(initialHeight, RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y);
	//}

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void FlyoutHeightAndWidthIncreaseAndDecreaseCorrectly()
	//{
	//	RunningApp.WaitForElement("PageLoaded");
	//	this.ShowFlyout();
	//	TapInFlyout("ChangeFlyoutSizes", makeSureFlyoutStaysOpen: true);
	//	var initialWidth = RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width;
	//	var initialHeight = RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y;
	//	TapInFlyout("DecreaseFlyoutSizes", makeSureFlyoutStaysOpen: true);
	//	var newWidth = RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width;
	//	var newHeight = RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y;

	//	Assert.That(initialWidth - newWidth, Is.EqualTo(10).Within(1));
	//	Assert.That(initialHeight - newHeight, Is.EqualTo(10).Within(1));
	//}
}