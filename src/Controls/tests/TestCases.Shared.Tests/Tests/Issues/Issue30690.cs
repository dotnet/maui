using NUnit.Framework;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.RefreshView)]
public class Issue30690 : _IssuesUITest
{
	private const string ToggleIsEnabledButton = "ToggleIsEnabled";
	private const string CheckStatesButton = "CheckStates";
	private const string TestEntry = "TestEntry";
	private const string ToggleIsRefreshEnabledButton = "ToggleIsRefreshEnabled";
	private const string Element = "StartRefresh";
	private const string StatusLabel = "StatusLabel";
	private const string ScrollViewContent = "ScrollViewContent";

	public Issue30690(TestDevice device) : base(device)
	{
	}

	public override string Issue => "RefreshView IsRefreshEnabled Property and Consistent IsEnabled Behavior";

	protected override bool ResetAfterEachTest => true;

	[Test]
	public void CheckInitialStates()
	{
		App.WaitForElement(CheckStatesButton);
		App.Tap(CheckStatesButton);

		Assert.That(GetStatusText(), Contains.Substring("IsEnabled: True"));
		Assert.That(GetStatusText(), Contains.Substring("IsRefreshEnabled: True"));
		Assert.That(GetStatusText(), Contains.Substring("IsRefreshing: False"));

		App.WaitForElement(TestEntry);
		App.Tap(TestEntry);
		App.EnterText(TestEntry, "test input");
		Assert.That(App.FindElement(TestEntry).GetText(), Is.EqualTo("test input"));
	}

	[Test]
	public void IsRefreshEnabledPreventsRefresh()
	{
		// Disable IsRefreshEnabled
		App.WaitForElement(ToggleIsRefreshEnabledButton);
		App.Tap(ToggleIsRefreshEnabledButton);
		Assert.That(GetStatusText(), Contains.Substring("IsRefreshEnabled: False"));

		// Try to start refresh
		App.Tap(Element);

		// Check that refresh did not start
		App.Tap(CheckStatesButton);
		Assert.That(GetStatusText(), Contains.Substring("IsRefreshing: False"));
	}

	[Test]
	public void IsRefreshEnabledAllowsChildInteraction()
	{
		// Disable IsRefreshEnabled
		App.WaitForElement(ToggleIsRefreshEnabledButton);
		App.Tap(ToggleIsRefreshEnabledButton);
		Assert.That(GetStatusText(), Contains.Substring("IsRefreshEnabled: False"));

		// Entry should still be interactive
		App.Tap(TestEntry);
		App.ClearText(TestEntry);
		App.EnterText(TestEntry, "refresh disabled but entry works");
		Assert.That(App.FindElement(TestEntry).GetText(), Is.EqualTo("refresh disabled but entry works"));
	}

	[Test]
	public void IsEnabledDisablesEntireViewAndPreventsRefresh()
	{
		// Disable IsEnabled
		App.WaitForElement(ToggleIsEnabledButton);
		App.Tap(ToggleIsEnabledButton);
		Assert.That(GetStatusText(), Contains.Substring("IsEnabled: False"));

		// Try to start refresh
		App.Tap(Element);

		// Check that refresh did not start
		App.Tap(CheckStatesButton);
		Assert.That(GetStatusText(), Contains.Substring("IsRefreshing: False"));
	}

	[Test]
	public void IsEnabledDisablesEntireViewAndPreventsChildInteraction()
	{
		// Disable IsEnabled
		App.WaitForElement(ToggleIsEnabledButton);
		App.Tap(ToggleIsEnabledButton);
		Assert.That(GetStatusText(), Contains.Substring("IsEnabled: False"));

		// Entry should not be interactive
		App.Tap(TestEntry);
		Assert.That(App.FindElement(TestEntry).IsEnabled(), Is.EqualTo(false));
		try
		{
			App.ClearText(TestEntry);
			App.EnterText(TestEntry, "this test input");
		}
		catch (InvalidElementStateException) // Expected as the entry should not be interactable
		{
		}
#if WINDOWS
		Assert.That(App.FindElement(TestEntry).GetText(), Is.EqualTo(""));
#else
		Assert.That(App.FindElement(TestEntry).GetText(), Is.EqualTo("Type here to test child interaction"));
#endif
	}

#if TEST_FAILS_ON_CATALYST // App.ScrollUp does nothing: https://github.com/dotnet/maui/issues/31216
	[Test]
	public void PullToRefreshWorksWhenEnabled()
	{
		// Find the scroll view content to perform pull gesture on
		App.WaitForElement(ScrollViewContent);
		bool refreshSucceeded = false;
		for (int i = 0; i < 3 && !refreshSucceeded; i++)
		{
			// Perform pull-to-refresh
			App.ScrollUp(ScrollViewContent, ScrollStrategy.Gesture, swipePercentage: 0.90, swipeSpeed: 1000);
			try
			{
				// Wait for refresh to complete and verify it worked
				App.WaitForTextToBePresentInElement(StatusLabel, "Refreshing...", timeout: TimeSpan.FromSeconds(5));
				refreshSucceeded = App.WaitForTextToBePresentInElement(StatusLabel, "Refresh completed", timeout: TimeSpan.FromSeconds(5));
			}

			catch (Exception)
			{
				// Refresh not completed yet, will retry
			}
		}
		Assert.That(refreshSucceeded, Is.True, "Pull-to-refresh did not complete after multiple attempts");
	}

	[Test]
	public void PullToRefreshBlockedWhenIsRefreshEnabledFalse()
	{
		// Disable IsRefreshEnabled
		App.WaitForElement(ToggleIsRefreshEnabledButton);
		App.Tap(ToggleIsRefreshEnabledButton);

		// Perform pull-to-refresh
		App.ScrollUp(ScrollViewContent);

		// Wait for refresh to complete and verify it failed
		Assert.That(GetStatusText(), Contains.Substring("IsRefreshEnabled: False"));
	}

	[Test]
	public void PullToRefreshBlockedWhenIsEnabledFalse()
	{
		// Disable IsEnabled
		App.WaitForElement(ToggleIsEnabledButton);
		App.Tap(ToggleIsEnabledButton);

		// Perform pull-to-refresh
		App.ScrollUp(ScrollViewContent);

		// Wait for refresh to complete and verify it failed
		Assert.That(GetStatusText(), Contains.Substring("IsEnabled: False"));
	}
#endif

	string GetStatusText() =>
		App.FindElement(StatusLabel).GetText() ?? "";

}
