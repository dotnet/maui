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

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutContentTests()
	{
		App.WaitForElement("PageLoaded");
		App.TapInShellFlyout("FlyoutItem");
		App.Tap("ToggleContent");
		App.TapInShellFlyout("ContentView");
		App.Tap("FlyoutItem");
		App.Tap("ToggleFlyoutContentTemplate");
		App.TapInShellFlyout("Reset");
		App.Tap("FlyoutItem");
	}
}
