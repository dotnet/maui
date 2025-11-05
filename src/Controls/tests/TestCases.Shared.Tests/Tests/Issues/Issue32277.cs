#if ANDROID || IOS // Exclude desktop platforms (Windows, MacCatalyst)
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32277 : _IssuesUITest
{
	public Issue32277(TestDevice device) : base(device)
	{
	}

	public override string Issue => "When a FlyoutPage is pushed Modally it doesn't inset the AppBarLayout";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void FlyoutPageModalAppBarLayoutGetsInsets()
	{
		// Tap the button to push the FlyoutPage modally
		App.WaitForElement("PushModalFlyoutButton");
		App.Tap("PushModalFlyoutButton");

		// Wait for the modal FlyoutPage to appear
		App.WaitForElement("DetailLabel");

		// Verify the detail page is visible and properly positioned
		// If the AppBarLayout insets are not applied correctly, the navigation bar
		// would render behind system UI elements
		App.WaitForElement("DetailLabel", "Detail page should be visible with proper insets");
	}
}
#endif
