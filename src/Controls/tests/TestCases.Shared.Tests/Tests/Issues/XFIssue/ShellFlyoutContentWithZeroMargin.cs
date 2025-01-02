#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_ANDROID //The flyout content with zero margin is offset by ignoring the safe area, and this behavior is specific to iOS. This test is expected to fail on Windows, Catalyst, and Android platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class ShellFlyoutContentWithZeroMargin : _IssuesUITest
{
	public ShellFlyoutContentWithZeroMargin(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Content With Zero Margin offsets correctly";

	[Test]
	public void FlyoutContentIgnoresSafeAreaWithZeroMargin()
	{
		App.WaitForElement("PageLoaded");
		App.ShowFlyout();
		var flyoutLocation = App.WaitForElement("FlyoutLabel").GetRect().Y;
		Assert.That(flyoutLocation, Is.EqualTo(0));
	}
}
#endif