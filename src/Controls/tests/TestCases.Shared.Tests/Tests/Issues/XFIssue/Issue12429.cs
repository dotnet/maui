using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12429 : _IssuesUITest
{
	public Issue12429(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell flyout items have a minimum height";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void FlyoutItemSizesToExplicitHeight()
	//{
	//	App.WaitForElement("PageLoaded");
	//	this.ShowFlyout();
	//	var height = App.WaitForElement("SmallFlyoutItem")[0].Rect.Height;
	//	Assert.That(height, Is.EqualTo(SmallFlyoutItem).Within(1));
	//}

	//[Test]
	//public void FlyoutItemHeightAndWidthIncreaseAndDecreaseCorrectly()
	//{
	//	App.WaitForElement("PageLoaded");
	//	this.ShowFlyout();
	//	var initialHeight = App.WaitForElement("ResizeMe")[0].Rect.Height;

	//	TapInFlyout("ResizeFlyoutItem", makeSureFlyoutStaysOpen: true);
	//	var newHeight = App.WaitForElement("ResizeMe")[0].Rect.Height;
	//	Assert.That(newHeight - initialHeight, Is.EqualTo(SizeToModifyBy).Within(1));

	//	TapInFlyout("ResizeFlyoutItemDown", makeSureFlyoutStaysOpen: true);
	//	newHeight = App.WaitForElement("ResizeMe")[0].Rect.Height;
	//	Assert.That(initialHeight, Is.EqualTo(newHeight).Within(1));

	//	TapInFlyout("ResizeFlyoutItemDown", makeSureFlyoutStaysOpen: true);
	//	newHeight = App.WaitForElement("ResizeMe")[0].Rect.Height;
	//	Assert.That(initialHeight - newHeight, Is.EqualTo(SizeToModifyBy).Within(1));

	//}
}