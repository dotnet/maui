#if MACCATALYST || WINDOWS  //MenuBarItem is only supported on macOS and UWP
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34038 : _IssuesUITest
{
	const string MenuEnabledSwitch = "Issue34038MenuEnabledSwitch";
	const string MenuBarItemText = "Issue34038MenuBarItem";
	const string MenuFlyoutItemText = "Issue34038MenuFlyoutItem";
	const string StatusLabel = "Issue34038StatusLabel";
	const string ExecutionCountLabel = "Issue34038ExecutionCount";

	public Issue34038(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[macOS] IsEnabled property false not working on MenuBarItem";

	[Test]
	[Category(UITestCategories.Shell)]
	public void DisabledMenuBarItemCannotBeOpenedOrExecuted()
	{
		// Navigate to test page
		App.WaitForElement("Issue34038NavigateButton");
		App.Tap("Issue34038NavigateButton");
		Assert.That(App.WaitForElement(StatusLabel).GetText(), Is.EqualTo("Failure"));
		Assert.That(App.WaitForElement(ExecutionCountLabel).GetText(), Is.EqualTo("0"));

#if WINDOWS
		App.Click(MenuBarItemText);
		App.WaitForNoElement(MenuFlyoutItemText);
		Assert.That(App.WaitForElement(StatusLabel).GetText(), Is.EqualTo("Failure"));
		App.WaitForElement(MenuEnabledSwitch);
		App.Tap(MenuEnabledSwitch);
		App.Click(MenuBarItemText);
		App.Click(MenuFlyoutItemText);
#else
		var menu = AppiumQuery.ByXPath($"//XCUIElementTypeMenuBarItem[@title='{MenuBarItemText}']");
		var item = AppiumQuery.ByXPath($"//XCUIElementTypeMenuItem[@title='{MenuFlyoutItemText}']");
		App.WaitForElement(menu);
		App.WaitForElement(item);
		// Catalyst disables the native menu actions, not the AppKit top-level menu.
		// Clicking a disabled action through WebDriver waits for it to become enabled.
		Assert.That(() => App.FindElement(item).IsEnabled(), Is.False.After(5000, 100));
		Assert.That(App.WaitForElement(StatusLabel).GetText(), Is.EqualTo("Failure"));
		App.WaitForElement(MenuEnabledSwitch);
		App.Tap(MenuEnabledSwitch);
		Assert.That(() => App.FindElement(item).IsEnabled(), Is.True.After(5000, 100));
		App.Tap(menu);
		Assert.That(() =>
		{
			var bounds = App.FindElement(item).GetRect();
			return bounds.Width > 0 && bounds.Height > 0;
		}, Is.True.After(5000, 100), "The enabled menu action must be open before selecting it.");
		App.Tap(item);
#endif
		Assert.That(() => App.FindElement(StatusLabel).GetText(), Is.EqualTo("Success").After(5000, 100));
		Assert.That(() => App.FindElement(ExecutionCountLabel).GetText(), Is.EqualTo("1").After(5000, 100));
#if MACCATALYST
		App.Tap(MenuEnabledSwitch);
		Assert.That(() => App.FindElement(item).IsEnabled(), Is.False.After(5000, 100));
		Assert.That(App.FindElement(ExecutionCountLabel).GetText(), Is.EqualTo("1"));
#endif
	}
}
#endif