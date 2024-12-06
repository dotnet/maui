using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellFlyoutContent : _IssuesUITest
{
	public ShellFlyoutContent(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Content";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void FlyoutContentTests()
	//{
	//	App.WaitForElement("PageLoaded");
	//	TapInFlyout("Flyout Item");
	//	App.Tap("ToggleContent");
	//	TapInFlyout("ContentView");
	//	TapInFlyout("Flyout Item");
	//	App.Tap("ToggleFlyoutContentTemplate");
	//	TapInFlyout("ContentView");
	//	TapInFlyout("Flyout Item");
	//}
}