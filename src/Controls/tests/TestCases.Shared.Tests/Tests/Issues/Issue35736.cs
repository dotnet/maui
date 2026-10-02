using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35736 : _IssuesUITest
{
	public Issue35736(TestDevice device) : base(device) { }

	public override string Issue => "SearchHandler QueryIcon, ClearIcon, ClearPlaceholderIcon need to update visually at runtime";

	protected override bool ResetAfterEachTest => true;

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerQueryIconUpdatesAtRuntime()
	{
		App.WaitForElement("Issue35736QueryIconLabel");

		// These states used to leak in from the two clear-icon tests.
		if (App.GetTestDevice() != TestDevice.Windows)
		{
			App.Tap("Issue35736ToggleClearIcon");
			App.Tap("Issue35736ToggleClearPlaceholderIcon");
		}

		App.Tap("Issue35736ToggleQueryIcon");
		Assert.That(App.WaitForTextToBePresentInElement("Issue35736QueryIconLabel", "QueryIcon: calculator.png"), Is.True);
		EnterQueryForVisibleIcons();

		VerifyIconScenario();
	}

	[Test]
	[Category(UITestCategories.Shell)]
#if WINDOWS
	[Ignore("ClearPlaceholderIcon is not displayed in Shell SearchHander : https://github.com/dotnet/maui/issues/28619")]
#endif
	public void SearchHandlerClearPlaceholderIconUpdatesAtRuntime()
	{
		App.WaitForElement("Issue35736ClearPlaceholderIconLabel");

		App.Tap("Issue35736ToggleClearIcon");
		App.Tap("Issue35736ToggleClearPlaceholderIcon");
		Assert.That(App.WaitForTextToBePresentInElement("Issue35736ClearPlaceholderIconLabel", "ClearPlaceholderIcon: calculator.png"), Is.True);
		EnterQueryForVisibleIcons();

		VerifyIconScenario();
	}

	[Test]
	[Category(UITestCategories.Shell)]
#if WINDOWS
	[Ignore("ClearIcon is not displayed in Shell SearchHander : https://github.com/dotnet/maui/issues/28619")]
#endif
	public void SearchHandlerClearIconUpdatesAtRuntime()
	{
		App.WaitForElement("Issue35736ClearIconLabel");

		App.Tap("Issue35736ToggleClearIcon");
		// Type text so the clear (X) button becomes visible
		App.EnterTextInShellSearchHandler("A");

		Assert.That(App.WaitForTextToBePresentInElement("Issue35736ClearIconLabel", "ClearIcon: calculator.png"), Is.True);

		VerifyIconScenario();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerResetAllRestoresDefaultIcons()
	{
		App.WaitForElement("Issue35736QueryIconLabel");

		// Change all icons first (including ClearIcon)
		App.Tap("Issue35736ToggleQueryIcon");
		App.Tap("Issue35736ToggleClearIcon");
		App.Tap("Issue35736ToggleClearPlaceholderIcon");

		// Reset all back to defaults
		App.Tap("Issue35736ResetAll");
		Assert.That(App.WaitForTextToBePresentInElement("Issue35736QueryIconLabel", "QueryIcon: default"), Is.True);
		Assert.That(App.WaitForTextToBePresentInElement("Issue35736ClearIconLabel", "ClearIcon: default"), Is.True);
		Assert.That(App.WaitForTextToBePresentInElement("Issue35736ClearPlaceholderIconLabel", "ClearPlaceholderIcon: default"), Is.True);
		EnterQueryForVisibleIcons();

		VerifyIconScenario();
	}

	void EnterQueryForVisibleIcons()
	{
		if (App.GetTestDevice() != TestDevice.Windows)
			App.EnterTextInShellSearchHandler("A");
	}

	void VerifyIconScenario()
	{
#if IOS || MACCATALYST
		// ShellSearchHandlerRegressionTests verifies native icon images, control states and reset.
		// Keep the input smoke check here without comparing UIKit's blinking caret.
		Assert.That(App.GetShellSearchHandler().GetText(), Is.EqualTo("A"));
#else
		VerifyScreenshot();
#endif
	}
}
