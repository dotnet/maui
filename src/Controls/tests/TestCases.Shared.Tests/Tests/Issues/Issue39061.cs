#if IOS || MACCATALYST // Regression is specific to the iOS/Mac Catalyst compatibility NavigationRenderer and UIKit toolbar items.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39061 : _IssuesUITest
{
	public Issue39061(TestDevice device) : base(device) { }

	public override string Issue => "ToolbarItem custom TintColor is lost when IsEnabled changes on iOS";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemPreservesCustomTintColorOnStateToggle()
	{
		App.WaitForElement("ToggleIsEnabledButton");

		App.Tap("ToggleIsEnabledButton");

		App.TapDisplayAlertButton("Dismiss");

		App.WaitForElement("SaveItem");

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
