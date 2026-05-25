using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21646_ShellFlyout: _IssuesUITest
{
	public Issue21646_ShellFlyout(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout icon should remain visible when a page is pushed onto a ShellPage with the back button disabled.";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void FlyoutIconShouldBeVisibleWithBackButtonDisabledInShellPage()
	{
		App.WaitForElement("NavigateToNextPageButton");
		App.Tap("NavigateToNextPageButton");
		App.WaitForElement("SecondPageLabel");
		App.WaitForFlyoutIcon(FlyoutIconAutomationId);
	}
}