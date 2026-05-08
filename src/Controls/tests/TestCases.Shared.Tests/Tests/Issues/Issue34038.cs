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

	public Issue34038(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[macOS] IsEnabled property false not working on MenuBarItem";

	[Test]
	[Category(UITestCategories.IsEnabled)]
	public void DisabledMenuBarItemCannotBeOpenedOrExecuted()
	{
        App.WaitForElement(MenuBarItemText);
		App.Tap(MenuBarItemText);

		App.WaitForElement(MenuFlyoutItemText);
        App.Tap(MenuFlyoutItemText);
		Assert.That(App.WaitForElement(StatusLabel).GetText(), Is.EqualTo("Failure"));
        App.Tap(MenuBarItemText);
		App.WaitForElement(MenuEnabledSwitch);

		App.Tap(MenuEnabledSwitch);

		App.WaitForElement(MenuBarItemText);
		App.Tap(MenuBarItemText);

		App.WaitForElement(MenuFlyoutItemText);
        App.Tap(MenuFlyoutItemText);
		Assert.That(App.WaitForElement(StatusLabel).GetText(), Is.EqualTo("Success"));
	}
}
#endif