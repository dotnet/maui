using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35736 : _IssuesUITest
{
	public Issue35736(TestDevice device) : base(device) { }

	public override string Issue => "SearchHandler QueryIcon, ClearIcon, ClearPlaceholderIcon need to update visually at runtime";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerQueryIconUpdatesAtRuntime()
	{
		App.WaitForElement("Issue35736QueryIconLabel");

		App.Tap("Issue35736ToggleQueryIcon");
		App.WaitForElement("Issue35736QueryIconLabel");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
#if WINDOWS
	[Ignore("ClearPlaceholderIcon is not displayed in Shell SearchHander : https://github.com/dotnet/maui/issues/28619")]
#endif
	public void SearchHandlerClearPlaceholderIconUpdatesAtRuntime()
	{
		App.WaitForElement("Issue35736ClearPlaceholderIconLabel");

		App.Tap("Issue35736ToggleClearPlaceholderIcon");
		App.WaitForElement("Issue35736ClearPlaceholderIconLabel");

		VerifyScreenshot();
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

		App.WaitForElement("Issue35736ClearIconLabel");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerResetAllRestoresDefaultIcons()
	{
		App.WaitForElement("Issue35736QueryIconLabel");

		// Change all icons first
		App.Tap("Issue35736ToggleQueryIcon");
		App.Tap("Issue35736ToggleClearPlaceholderIcon");

		// Reset all back to defaults
		App.Tap("Issue35736ResetAll");
		App.WaitForElement("Issue35736QueryIconLabel");

		VerifyScreenshot();
	}
}
