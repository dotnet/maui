#if TEST_FAILS_ON_WINDOWS // On Windows, Shell custom flyout-item taps are not working in this scenario, so this issue test is excluded.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34931 : _IssuesUITest
{
	public Issue34931(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "Shell flyout item template does not update selected visuals after DynamicResource changes";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutSelectedStateReflectsUpdatedDynamicResource()
	{
		App.WaitForElement("ChangeColorButton");
		App.Tap("ChangeColorButton");
		Assert.That(App.WaitForElement("CurrentColorLabel").GetText(), Does.Contain("#FF6347"));
		NavigateWithFlyout("Second", "Issue34931SecondPageLabel");
		NavigateWithFlyout("Third", "Issue34931ThirdPageLabel");
		NavigateWithFlyout("Home", "ChangeColorButton");
		App.Tap("ChangeColorButton");
		App.TapShellFlyoutIcon();
		App.WaitForElement("Third");
		VerifyScreenshot();
	}

	void NavigateWithFlyout(string flyoutItemTitle, string pageReadyElement)
	{
		App.TapShellFlyoutIcon();
		App.WaitForElement(flyoutItemTitle);
		App.Tap(flyoutItemTitle);
		App.WaitForElement(pageReadyElement);
	}
}
#endif
