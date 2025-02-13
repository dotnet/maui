#if TEST_FAILS_ON_WINDOWS // On Windows AutomationId is not working for Stacklayout, Hence we measure the layout height here so we can't use the inner elements AutomationId. More Information:https://github.com/dotnet/maui/issues/4715
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12429 : _IssuesUITest
{
	public Issue12429(TestDevice testDevice) : base(testDevice)
	{

	}

#if ANDROID // AutomationId not works iOS and Catalyst, hence using the text of the element.
	const string SmallFlyoutItem = "SmallFlyoutItem";
#else
    const string SmallFlyoutItem="I'm set to specific height: ";
#endif

#if ANDROID || IOS // Rect value measurements from Appium vary across platforms; these values ensure consistent behavior
	double SmallFlyoutItemValue = 35d;
	double SizeToModifyBy = 20d;
#elif MACCATALYST
    double SmallFlyoutItemValue=28d;
    double SizeToModifyBy=15d;
#endif
	const string ResizeMe = "Default Flyout Item. Height is 44 on iOS and UWP. Height is 50 on Android)";



	public override string Issue => "[Bug] Shell flyout items have a minimum height";

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void FlyoutItemSizesToExplicitHeight()
	{
		App.WaitForElement("PageLoaded");
		App.ShowFlyout();
		var height = App.WaitForElement(SmallFlyoutItem).GetRect();
		Assert.That(height.Height, Is.EqualTo(SmallFlyoutItemValue).Within(1));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void FlyoutItemHeightAndWidthIncreaseAndDecreaseCorrectly()
	{
		App.WaitForElement(ResizeMe);
		var initialHeight = App.WaitForElement(ResizeMe).GetRect().Y;

		App.Tap("ResizeFlyoutItem");
		var newHeight = App.WaitForElement(ResizeMe).GetRect().Y;
		Assert.That(newHeight - initialHeight, Is.EqualTo(SizeToModifyBy).Within(1));

		App.Tap("ResizeFlyoutItemDown");
		newHeight = App.WaitForElement(ResizeMe).GetRect().Y;
		Assert.That(initialHeight, Is.EqualTo(newHeight).Within(1));

		App.Tap("ResizeFlyoutItemDown");
		newHeight = App.WaitForElement(ResizeMe).GetRect().Y;
		Assert.That(initialHeight - newHeight, Is.EqualTo(SizeToModifyBy).Within(1));

	}
}
#endif
