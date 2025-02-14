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

#if WINDOWS
    const string ChangeFlyoutSizes="Change Height and Width";
    const string ResetFlyoutSizes="Reset Height and Width";
    const string DecreaseFlyoutSizes="Decrease Height and Width";
#else
	const string ChangeFlyoutSizes = "ChangeFlyoutSizes";
	const string ResetFlyoutSizes = "ResetFlyoutSizes";
	const string DecreaseFlyoutSizes = "DecreaseFlyoutSizes";
#endif

#if ANDROID // Appium's GetRect method returns different sizes across platforms.
	int difference = 26;
#elif IOS || WINDOWS
	int difference = 10;
#elif MACCATALYST
    int difference = 8;
#endif

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void FlyoutHeightAndWidthResetsBackToOriginalSize()
	{
		App.WaitForElement("PageLoaded");
		var initialWidth = App.WaitForElement("FlyoutHeader").GetRect().Width;
		var initialHeight = App.WaitForElement("FlyoutFooter").GetRect().Y;
		App.Tap(ChangeFlyoutSizes);
		Assert.That(App.WaitForElement("FlyoutHeader").GetRect().Width, Is.Not.EqualTo(initialWidth));
		Assert.That(App.WaitForElement("FlyoutFooter").GetRect().Y, Is.Not.EqualTo(initialHeight));

		App.Tap(ResetFlyoutSizes);
		Assert.That(App.WaitForElement("FlyoutHeader").GetRect().Width, Is.EqualTo(initialWidth));
		Assert.That(App.WaitForElement("FlyoutFooter").GetRect().Y, Is.EqualTo(initialHeight));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void FlyoutHeightAndWidthIncreaseAndDecreaseCorrectly()
	{
		App.WaitForElement(ChangeFlyoutSizes);
		App.Tap(ChangeFlyoutSizes);
		var initialWidth = App.WaitForElement("FlyoutHeader").GetRect().Width;
		var initialHeight = App.WaitForElement("FlyoutFooter").GetRect().Y;
		App.Tap(DecreaseFlyoutSizes);
		var newWidth = App.WaitForElement("FlyoutHeader").GetRect().Width;
		var newHeight = App.WaitForElement("FlyoutFooter").GetRect().Y;
		Assert.That(initialWidth - newWidth, Is.EqualTo(difference).Within(1));
		Assert.That(initialHeight - newHeight, Is.EqualTo(difference).Within(1));

	}
}