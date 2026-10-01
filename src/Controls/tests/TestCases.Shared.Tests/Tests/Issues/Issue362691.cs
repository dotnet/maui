#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue362691 : _IssuesUITest
{
	public Issue362691(TestDevice device) : base(device) { }

	public override string Issue => "ToolbarItem custom TintColor is lost when IsEnabled changes on iOS";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemPreservesCustomTintColorOnStateToggle()
	{
		App.WaitForElement("ToggleIsEnabledButton");

		App.Tap("ToggleIsEnabledButton");

		App.WaitForElement("Dismiss");
		App.Tap("Dismiss");

		App.WaitForElement("SaveItem");

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
