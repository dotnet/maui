#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Scroll not supported on MacCatalyst and On Windows, AutomationId is not working for Stacklayout, Hence we measure the layout height here so we can't use the inner elements AutomationId. More Information:https://github.com/dotnet/maui/issues/4715
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellFlyoutHeaderBehavior : _IssuesUITest
{
	public ShellFlyoutHeaderBehavior(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Header Behavior";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutHeaderBehaviorFixed()
	{
		App.WaitForElement("Fixed");
		App.Tap("Fixed");
		float startingHeight = GetFlyoutHeight();
		App.ScrollDown("Item 4", ScrollStrategy.Gesture);
		float endHeight = GetFlyoutHeight();

		Assert.That(startingHeight, Is.EqualTo(endHeight).Within(1));
	}
#if !IOS // For iOS, getting incorrect Rect values from GetRect method in Appium even though the size is reduced in UI.
	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutHeaderBehaviorCollapseOnScroll()
	{
		App.WaitForElement("CollapseOnScroll");
		App.Tap("CollapseOnScroll");
		float startingHeight = GetFlyoutHeight();
		App.ScrollDown("Item 4", ScrollStrategy.Gesture);
		float endHeight = GetFlyoutHeight();

		Assert.That(startingHeight, Is.GreaterThan(endHeight));
	}

	[Test] // Skip this for iOS, because FindElements returns count eventhough the element is scrolled up and hidded from the UI.
	[Category(UITestCategories.Shell)]
	public void FlyoutHeaderBehaviorScroll()
	{
		App.WaitForElement("Scroll");
		App.Tap("Scroll");
		var startingY = GetFlyoutY();
		App.ScrollDown("Item 5", ScrollStrategy.Gesture);
		var nextY = GetFlyoutY();

		while (nextY != null && startingY != null)
		{
			Assert.That(startingY.Value, Is.GreaterThanOrEqualTo(nextY.Value));
			startingY = nextY;
			App.ScrollDown("Item 5", ScrollStrategy.Gesture);
			nextY = GetFlyoutY();
		}
	}
#endif

	float GetFlyoutHeight() =>
		App.WaitForElement("FlyoutHeaderId").GetRect().Height;

	float? GetFlyoutY()
	{
		var flyoutHeader =
			App.FindElements("FlyoutHeaderId");

		foreach (var element in flyoutHeader)
		{
			return element.GetRect().Y;
		}
		return null;
	}
}
#endif
