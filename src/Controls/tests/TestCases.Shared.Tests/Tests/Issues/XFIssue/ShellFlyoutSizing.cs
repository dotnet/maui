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
	protected override bool ResetAfterEachTest => true;

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
		Assert.That(() => App.FindElement("FlyoutHeader").GetRect().Width, Is.Not.EqualTo(initialWidth).After(10000, 200));
		Assert.That(() => App.FindElement("FlyoutFooter").GetRect().Y, Is.Not.EqualTo(initialHeight).After(10000, 200));

		App.Tap(ResetFlyoutSizes);
		Assert.That(() => App.FindElement("FlyoutHeader").GetRect().Width, Is.EqualTo(initialWidth).After(10000, 200));
		Assert.That(() => App.FindElement("FlyoutFooter").GetRect().Y, Is.EqualTo(initialHeight).After(10000, 200));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void FlyoutHeightAndWidthIncreaseAndDecreaseCorrectly()
	{
		App.WaitForElement(ChangeFlyoutSizes);
		var originalWidth = App.FindElement("FlyoutHeader").GetRect().Width;
		App.Tap(ChangeFlyoutSizes);
		Assert.That(() => App.FindElement("FlyoutHeader").GetRect().Width, Is.Not.EqualTo(originalWidth).After(10000, 200));
		App.WaitForElement(DecreaseFlyoutSizes);
		var initialWidth = App.WaitForElement("FlyoutHeader").GetRect().Width;
		var initialHeight = App.WaitForElement("FlyoutFooter").GetRect().Y;
		App.Tap(DecreaseFlyoutSizes);
		Assert.That(() => initialWidth - App.FindElement("FlyoutHeader").GetRect().Width,
			Is.EqualTo(difference).Within(1).After(10000, 200));
		Assert.That(() => initialHeight - App.FindElement("FlyoutFooter").GetRect().Y,
			Is.EqualTo(difference).Within(1).After(10000, 200));

	}
}