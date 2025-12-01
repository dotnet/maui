using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellFlyoutContent : _IssuesUITest
{

#if WINDOWS //In Windows AutomationId for FlyoutItems not works in Appium.
	const string FlyoutItem = "Flyout Item Top";
	const string ResetButton = "Click to Reset";
#else
	const string FlyoutItem = "FlyoutItem";
	const string ResetButton = "Reset";
#endif

	public ShellFlyoutContent(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Content";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutContentTests()
	{
		App.WaitForElement("PageLoaded");
		App.TapInShellFlyout(FlyoutItem);
		App.Tap("ToggleContent");
		App.TapInShellFlyout("ContentView");
		App.Tap(FlyoutItem);
		App.Tap("ToggleFlyoutContentTemplate");
		App.TapInShellFlyout(ResetButton);
		App.Tap(FlyoutItem);
	}

	// https://github.com/dotnet/maui/issues/32883
	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutFooterAreaClearedAfterRemoval()
	{
		App.WaitForElement("PageLoaded");

		// Open flyout and get the position of the bottom flyout item before adding footer
		App.TapInShellFlyout("Flyout Item Bottom");

		// Add header and footer
		App.Tap("ToggleHeaderFooter");

		// Open the flyout and verify footer is visible
		App.TapInShellFlyout("Footer View");

		// Remove header and footer
		App.Tap("ToggleHeaderFooter");

		// Open flyout and verify the footer area is cleared by checking "Flyout Item Bottom" is still accessible
		// If the padding wasn't cleared, the bottom flyout item would be pushed up and inaccessible
		App.TapInShellFlyout("Flyout Item Bottom");
	}
}