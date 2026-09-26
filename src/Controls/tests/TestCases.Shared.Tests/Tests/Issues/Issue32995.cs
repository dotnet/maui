#if TEST_FAILS_ON_WINDOWS // Issue Link : https://github.com/dotnet/maui/issues/34738
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32995 : _IssuesUITest
{
	public Issue32995(TestDevice device) : base(device) { }

	public override string Issue => "TabBarDisabledColor not applied to disabled tabs on iOS";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarDisabledColorAppliedToDisabledTab()
	{
		App.WaitForElement(FindTab2, "Tab2 did not appear");
		VerifyScreenshot("DisabledTabWithGreenColor");

		App.Tap("EnableButton");
		App.RetryAssert(() =>
		{
			var tab = FindTab2();
			Assert.That(tab, Is.Not.Null, "Tab2 was not found after enabling it");
			Assert.That(tab.IsEnabled(), Is.True, "Tab2 did not become enabled");
		});
		VerifyScreenshot("EnabledTabWithNormalColor");
	}

	IUIElement FindTab2() =>
		App.GetTestDevice() == TestDevice.Android
			? App.FindElement(AppiumQuery.ByAccessibilityId("Tab2"))
			: App.FindElement("Tab2");
}
#endif
